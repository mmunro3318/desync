using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Desync.Tests.EditMode
{
    /// <summary>
    /// Regression tests for House_Graybox geometry constraints that prevent
    /// light leaks and Z-fighting between floors. Validates the construction
    /// rules documented in CLAUDE.md and ARCH.md:
    ///   - Floor/ceiling rects inset to wall inner edges
    ///   - Ceiling tops flush with wall tops
    ///   - TwoSided shadow casting on all floor/ceiling renderers
    /// </summary>
    public class HouseGrayboxGeometryTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/House_Graybox.unity";

        private const float Tolerance = 0.01f;

        private static readonly string[] FloorCeilingNames =
        {
            "GF_Floor", "GF_Ceiling",
            "SF_Floor_A", "SF_Floor_B", "SF_Floor_C", "SF_Ceiling"
        };

        private Scene _openedScene;

        [SetUp]
        public void OpenHouseGrayboxScene()
        {
            _openedScene = EditorSceneManager.OpenScene(
                ScenePath, OpenSceneMode.Single);
            Assert.IsTrue(
                _openedScene.IsValid() && _openedScene.isLoaded,
                $"Failed to open scene at '{ScenePath}'.");

            // Preflight: verify expected root objects exist before tests run.
            Assert.IsNotNull(GameObject.Find("GF_Walls_Exterior"),
                "Scene opened but GF_Walls_Exterior not found — scene may be corrupt or stale.");
        }

        [TearDown]
        public void RestoreCleanScene()
        {
            EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void FloorCeilingBoundsWithinWallMidpoints()
        {
            // Grammar R1.3: separator XZ edges extend to wall MIDPOINT,
            // not inner edge. Midpoint = (inner + outer) / 2 per wall.
            var midpointBounds = ComputeExteriorWallMidpointBounds();
            var floorCeilings = FindFloorCeilingRenderers();

            foreach (var renderer in floorCeilings)
            {
                var b = renderer.bounds;
                var name = renderer.gameObject.name;

                Assert.GreaterOrEqual(b.min.x, midpointBounds.min.x - Tolerance,
                    $"{name} bounds.min.x ({b.min.x:F4}) extends past wall midpoint ({midpointBounds.min.x:F4}).");
                Assert.LessOrEqual(b.max.x, midpointBounds.max.x + Tolerance,
                    $"{name} bounds.max.x ({b.max.x:F4}) extends past wall midpoint ({midpointBounds.max.x:F4}).");
                Assert.GreaterOrEqual(b.min.z, midpointBounds.min.z - Tolerance,
                    $"{name} bounds.min.z ({b.min.z:F4}) extends past wall midpoint ({midpointBounds.min.z:F4}).");
                Assert.LessOrEqual(b.max.z, midpointBounds.max.z + Tolerance,
                    $"{name} bounds.max.z ({b.max.z:F4}) extends past wall midpoint ({midpointBounds.max.z:F4}).");
            }
        }

        [Test]
        public void CeilingTopsAboveWallTops()
        {
            // Grammar R1.2: separator top = wall top + 0.05m.
            // Ceiling must be ABOVE walls (walls terminate INTO the slab).
            const float capHeight = 0.05f;

            AssertCeilingCapAboveWalls(
                "GF_Ceiling", "GF_Walls_Exterior", capHeight,
                "GF_Ceiling");

            AssertCeilingCapAboveWalls(
                "SF_Ceiling", "SF_Walls_Exterior", capHeight,
                "SF_Ceiling");
        }

        [Test]
        public void NoCoplanarFacesBetweenGFCeilingAndSFFloor()
        {
            var gfCeiling = FindRequiredObject("GF_Ceiling");
            var gfCeilingRenderer = gfCeiling.GetComponent<MeshRenderer>();
            Assert.IsNotNull(gfCeilingRenderer,
                "GF_Ceiling has no MeshRenderer.");

            float ceilingTopY = gfCeilingRenderer.bounds.max.y;

            string[] sfFloorNames = { "SF_Floor_A", "SF_Floor_B", "SF_Floor_C" };
            foreach (var floorName in sfFloorNames)
            {
                var floor = FindRequiredObject(floorName);
                var floorRenderer = floor.GetComponent<MeshRenderer>();
                Assert.IsNotNull(floorRenderer,
                    $"{floorName} has no MeshRenderer.");

                float floorBottomY = floorRenderer.bounds.min.y;
                float gap = Mathf.Abs(ceilingTopY - floorBottomY);

                Assert.GreaterOrEqual(gap, Tolerance,
                    $"GF_Ceiling top ({ceilingTopY:F4}) and {floorName} bottom ({floorBottomY:F4}) " +
                    $"are coplanar (gap {gap:F4} < {Tolerance}) — Z-fighting risk.");
            }
        }

        [Test]
        public void AllFloorCeilingRenderersUseTwoSidedShadows()
        {
            foreach (var objName in FloorCeilingNames)
            {
                var go = FindRequiredObject(objName);
                var renderer = go.GetComponent<MeshRenderer>();
                Assert.IsNotNull(renderer,
                    $"{objName} has no MeshRenderer.");

                Assert.AreEqual(ShadowCastingMode.TwoSided, renderer.shadowCastingMode,
                    $"{objName} shadowCastingMode is {renderer.shadowCastingMode}, expected TwoSided. " +
                    "Disabled renderers still need TwoSided for shadow containment.");
            }
        }

        [Test]
        public void InteriorBoundsAreSmallerThanExteriorAndNonDegenerate()
        {
            // Validates the inner-edge computation logic itself:
            // 1. Interior must be strictly smaller than the exterior envelope
            // 2. Interior must have positive area (not collapsed/degenerate)
            // 3. Every floor/ceiling piece must be inset at least MinInset
            //    units from the exterior — catches the original bug where
            //    floor side-faces protruded through walls.
            const float minInset = 0.05f;

            var wallRenderers = CollectExteriorWallRenderers();
            var outerBounds = wallRenderers[0].bounds;
            for (int i = 1; i < wallRenderers.Length; i++)
                outerBounds.Encapsulate(wallRenderers[i].bounds);

            var interiorBounds = ComputeExteriorWallInteriorBounds();

            // Interior strictly inside exterior on X and Z.
            Assert.Greater(interiorBounds.min.x, outerBounds.min.x,
                "Interior min X should be inside exterior min X (wall has thickness).");
            Assert.Less(interiorBounds.max.x, outerBounds.max.x,
                "Interior max X should be inside exterior max X (wall has thickness).");
            Assert.Greater(interiorBounds.min.z, outerBounds.min.z,
                "Interior min Z should be inside exterior min Z (wall has thickness).");
            Assert.Less(interiorBounds.max.z, outerBounds.max.z,
                "Interior max Z should be inside exterior max Z (wall has thickness).");

            // Non-degenerate: positive dimensions.
            float interiorWidth = interiorBounds.max.x - interiorBounds.min.x;
            float interiorDepth = interiorBounds.max.z - interiorBounds.min.z;
            Assert.Greater(interiorWidth, 0f, "Interior has zero or negative width.");
            Assert.Greater(interiorDepth, 0f, "Interior has zero or negative depth.");

            // Every floor/ceiling piece is inset from exterior.
            var floorCeilings = FindFloorCeilingRenderers();
            foreach (var renderer in floorCeilings)
            {
                var b = renderer.bounds;
                var name = renderer.gameObject.name;
                Assert.Greater(b.min.x - outerBounds.min.x, minInset,
                    $"{name} min X too close to exterior wall (inset {b.min.x - outerBounds.min.x:F4} < {minInset}).");
                Assert.Greater(outerBounds.max.x - b.max.x, minInset,
                    $"{name} max X too close to exterior wall (inset {outerBounds.max.x - b.max.x:F4} < {minInset}).");
                Assert.Greater(b.min.z - outerBounds.min.z, minInset,
                    $"{name} min Z too close to exterior wall (inset {b.min.z - outerBounds.min.z:F4} < {minInset}).");
                Assert.Greater(outerBounds.max.z - b.max.z, minInset,
                    $"{name} max Z too close to exterior wall (inset {outerBounds.max.z - b.max.z:F4} < {minInset}).");
            }
        }

        // --- Helpers ---

        /// <summary>
        /// Computes the midpoint bounding box (X/Z) from exterior walls.
        /// Per grammar R1.3, separators extend to wall midpoint, not inner face.
        /// Midpoint = center of the wall panel on its thin axis.
        /// </summary>
        private Bounds ComputeExteriorWallMidpointBounds()
        {
            var wallRenderers = CollectExteriorWallRenderers();
            Assert.IsTrue(wallRenderers.Length > 0,
                "No MeshRenderers found under exterior wall objects.");

            var outerBounds = wallRenderers[0].bounds;
            for (int i = 1; i < wallRenderers.Length; i++)
                outerBounds.Encapsulate(wallRenderers[i].bounds);
            Vector3 center = outerBounds.center;

            float midMinX = float.MinValue;
            float midMaxX = float.MaxValue;
            float midMinZ = float.MinValue;
            float midMaxZ = float.MaxValue;

            foreach (var wr in wallRenderers)
            {
                var wb = wr.bounds;
                bool thinInX = wb.size.x < wb.size.z;

                if (thinInX)
                {
                    float midpoint = wb.center.x;
                    if (wb.center.x < center.x)
                        midMinX = Mathf.Max(midMinX, midpoint);
                    else
                        midMaxX = Mathf.Min(midMaxX, midpoint);
                }
                else
                {
                    float midpoint = wb.center.z;
                    if (wb.center.z < center.z)
                        midMinZ = Mathf.Max(midMinZ, midpoint);
                    else
                        midMaxZ = Mathf.Min(midMaxZ, midpoint);
                }
            }

            Assert.AreNotEqual(float.MinValue, midMinX, "No min-X wall found.");
            Assert.AreNotEqual(float.MaxValue, midMaxX, "No max-X wall found.");
            Assert.AreNotEqual(float.MinValue, midMinZ, "No min-Z wall found.");
            Assert.AreNotEqual(float.MaxValue, midMaxZ, "No max-Z wall found.");

            var midBounds = new Bounds();
            midBounds.SetMinMax(
                new Vector3(midMinX, outerBounds.min.y, midMinZ),
                new Vector3(midMaxX, outerBounds.max.y, midMaxZ));
            return midBounds;
        }

        /// <summary>
        /// Computes the interior bounding box (X/Z) from exterior wall inner
        /// faces. Each wall panel is classified by its thin axis (X or Z) and
        /// its position relative to center. The inner face of each wall
        /// defines one edge of the allowed interior region.
        /// </summary>
        private Bounds ComputeExteriorWallInteriorBounds()
        {
            var wallRenderers = CollectExteriorWallRenderers();
            Assert.IsTrue(wallRenderers.Length > 0,
                "No MeshRenderers found under exterior wall objects.");

            // Compute building center from combined outer bounds.
            var outerBounds = wallRenderers[0].bounds;
            for (int i = 1; i < wallRenderers.Length; i++)
                outerBounds.Encapsulate(wallRenderers[i].bounds);
            Vector3 center = outerBounds.center;

            // Walk each wall panel. Thin axis determines which axis it
            // constrains; position relative to center determines which side.
            float innerMinX = float.MinValue;
            float innerMaxX = float.MaxValue;
            float innerMinZ = float.MinValue;
            float innerMaxZ = float.MaxValue;

            foreach (var wr in wallRenderers)
            {
                var wb = wr.bounds;
                bool thinInX = wb.size.x < wb.size.z;

                if (thinInX)
                {
                    if (wb.center.x < center.x)
                        innerMinX = Mathf.Max(innerMinX, wb.max.x);
                    else
                        innerMaxX = Mathf.Min(innerMaxX, wb.min.x);
                }
                else
                {
                    if (wb.center.z < center.z)
                        innerMinZ = Mathf.Max(innerMinZ, wb.max.z);
                    else
                        innerMaxZ = Mathf.Min(innerMaxZ, wb.min.z);
                }
            }

            Assert.AreNotEqual(float.MinValue, innerMinX, "No min-X wall found.");
            Assert.AreNotEqual(float.MaxValue, innerMaxX, "No max-X wall found.");
            Assert.AreNotEqual(float.MinValue, innerMinZ, "No min-Z wall found.");
            Assert.AreNotEqual(float.MaxValue, innerMaxZ, "No max-Z wall found.");

            var interior = new Bounds();
            interior.SetMinMax(
                new Vector3(innerMinX, outerBounds.min.y, innerMinZ),
                new Vector3(innerMaxX, outerBounds.max.y, innerMaxZ));
            return interior;
        }

        private MeshRenderer[] CollectExteriorWallRenderers()
        {
            var gfWalls = FindRequiredObject("GF_Walls_Exterior");
            var sfWalls = FindRequiredObject("SF_Walls_Exterior");

            var gfRenderers = gfWalls.GetComponentsInChildren<MeshRenderer>(true);
            var sfRenderers = sfWalls.GetComponentsInChildren<MeshRenderer>(true);

            var all = new MeshRenderer[gfRenderers.Length + sfRenderers.Length];
            gfRenderers.CopyTo(all, 0);
            sfRenderers.CopyTo(all, gfRenderers.Length);
            return all;
        }

        private MeshRenderer[] FindFloorCeilingRenderers()
        {
            var renderers = new MeshRenderer[FloorCeilingNames.Length];
            for (int i = 0; i < FloorCeilingNames.Length; i++)
            {
                var go = FindRequiredObject(FloorCeilingNames[i]);
                var r = go.GetComponent<MeshRenderer>();
                Assert.IsNotNull(r,
                    $"{FloorCeilingNames[i]} has no MeshRenderer.");
                renderers[i] = r;
            }
            return renderers;
        }

        private void AssertCeilingCapAboveWalls(
            string ceilingName, string wallsParentName,
            float expectedCap, string label)
        {
            var ceiling = FindRequiredObject(ceilingName);
            var ceilingRenderer = ceiling.GetComponent<MeshRenderer>();
            Assert.IsNotNull(ceilingRenderer,
                $"{ceilingName} has no MeshRenderer.");

            var wallsParent = FindRequiredObject(wallsParentName);
            var wallRenderers = wallsParent.GetComponentsInChildren<MeshRenderer>(true);
            Assert.IsTrue(wallRenderers.Length > 0,
                $"No MeshRenderers found under {wallsParentName}.");

            float wallMaxY = float.MinValue;
            foreach (var wr in wallRenderers)
            {
                if (wr.bounds.max.y > wallMaxY)
                    wallMaxY = wr.bounds.max.y;
            }

            float ceilingTop = ceilingRenderer.bounds.max.y;
            float expectedTop = wallMaxY + expectedCap;

            // Ceiling top must be above wall tops (R1.2: walls terminate INTO slab)
            Assert.GreaterOrEqual(ceilingTop, wallMaxY,
                $"{label} top ({ceilingTop:F4}) is below wall tops ({wallMaxY:F4}).");

            // Ceiling top must not exceed expected cap height + tolerance
            Assert.LessOrEqual(ceilingTop, expectedTop + Tolerance,
                $"{label} top ({ceilingTop:F4}) exceeds expected cap " +
                $"({expectedTop:F4} = wall top {wallMaxY:F4} + {expectedCap}m).");
        }

        private static GameObject FindRequiredObject(string name)
        {
            var go = GameObject.Find(name);
            Assert.IsNotNull(go,
                $"GameObject '{name}' not found in scene '{ScenePath}'.");
            return go;
        }
    }
}
