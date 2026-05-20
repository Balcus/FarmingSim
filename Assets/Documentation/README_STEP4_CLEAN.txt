GardenGuideStep4 - clean add-only package

Goal:
- Adds only the missing Step 3.5-3.8 scripts and Step 4 prefab-generation workflow.
- Uses the existing Normaldirt and Ploweddirt objects in SampleScene as the farming tiles.
- Does NOT generate a random grid in the field.
- Does NOT overwrite the existing GameManager, WeatherSystem, PlantData, PlayerMovement, MouseMovement, SelectionManager, or scene assets.

Install:
1. Import this package into a clean copy of the GitHub project.
2. Open Assets/Scenes/SampleScene.unity.
3. Run: Garden Guide > Step 4 > Install On Current Scene - Existing Dirt Only
4. Press Play.

Controls:
WASD / Mouse = existing project movement/look
1 = Hoe / Plow
2 = Watering Can
3 = Insecticide
4 = Fertilizer
5 = Harvest
6 = Seeds mode
7 = Tomato
8 = Carrot
9 = Lettuce
E or Left Click = interact with targeted tile
R = refill near RefilPoint / WaterWell
T = advance one in-game day for testing growth

Generated assets are placed under:
Assets/GardenGuideStep4/Generated/

Important:
If the scene already contains old AI-generated folders or objects from previous attempts, use a fresh GitHub copy before importing this package.
