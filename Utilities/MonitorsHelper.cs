﻿using GeneralImprovements.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GeneralImprovements.Utilities
{
    internal static class MonitorsHelper
    {
        public const int MonitorCount = 14;

        public static class MonitorNames
        {
            public const string ProfitQuota = "ProfitQuota";
            public const string Deadline = "Deadline";
            public const string ShipScrap = "ShipScrap";
            public const string Time = "Time";
            public const string Weather = "Weather";
            public const string FancyWeather = "FancyWeather";
            public const string Sales = "Sales";
            public const string InternalCam = "InternalCam";
            public const string ExternalCam = "ExternalCam";

            public static bool MonitorExists(string monitorName)
            {
                return GetMonitorIndex(monitorName) >= 0;
            }

            public static int GetMonitorIndex(string monitorName)
            {
                for (int i = 0; i < MonitorCount; i++)
                {
                    if (Plugin.ShipMonitorAssignments[i].Value == monitorName)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        private static Image _originalProfitQuotaBG;
        private static TextMeshProUGUI _originalProfitQuotaText;
        private static Image _originalDeadlineBG;
        private static TextMeshProUGUI _originalDeadlineText;

        private static List<Image> _profitQuotaBGs = new List<Image>();
        private static List<TextMeshProUGUI> _profitQuotaTexts = new List<TextMeshProUGUI>();
        private static List<Image> _deadlineBGs = new List<Image>();
        private static List<TextMeshProUGUI> _deadlineTexts = new List<TextMeshProUGUI>();
        private static List<Image> _shipScrapMonitorBGs = new List<Image>();
        private static List<TextMeshProUGUI> _shipScrapMonitorTexts = new List<TextMeshProUGUI>();
        private static List<Image> _timeMonitorBGs = new List<Image>();
        private static List<TextMeshProUGUI> _timeMonitorTexts = new List<TextMeshProUGUI>();
        private static List<Image> _weatherMonitorBGs = new List<Image>();
        private static List<TextMeshProUGUI> _weatherMonitorTexts = new List<TextMeshProUGUI>();
        private static List<Image> _fancyWeatherMonitorBGs = new List<Image>();
        private static List<TextMeshProUGUI> _fancyWeatherMonitorTexts = new List<TextMeshProUGUI>();
        private static List<Image> _salesMonitorBGs = new List<Image>();
        private static List<TextMeshProUGUI> _salesMonitorTexts = new List<TextMeshProUGUI>();
        private static List<Image> _extraBackgrounds = new List<Image>();

        private static Monitors _newMonitors;

        private static int _curWeatherAnimIndex = 0;
        private static int _curWeatherOverlayIndex = 0;
        private static string[] _curWeatherAnimations = new string[0];
        private static string[] _curWeatherOverlays = new string[0];

        private static float _animTimer = 0;
        private static float _animCycle = 0.2f; // In seconds
        private static bool _hasOverlays = false;
        private static float _overlayTimer = 0;
        private static bool _showingOverlay = false;
        private static float _overlayCycle; // In seconds, randomly assigned each time
        private static Transform _oldMonitorsObject;
        private static Transform _oldBigMonitors;
        private static Transform _UIContainer;
        private static ScanNodeProperties _profitQuotaScanNode;

        public static void CreateExtraMonitors()
        {
            // Initialize things each time StartOfRound starts up
            _profitQuotaBGs = new List<Image>();
            _profitQuotaTexts = new List<TextMeshProUGUI>();
            _deadlineBGs = new List<Image>();
            _deadlineTexts = new List<TextMeshProUGUI>();
            _shipScrapMonitorBGs = new List<Image>();
            _shipScrapMonitorTexts = new List<TextMeshProUGUI>();
            _timeMonitorBGs = new List<Image>();
            _timeMonitorTexts = new List<TextMeshProUGUI>();
            _weatherMonitorBGs = new List<Image>();
            _weatherMonitorTexts = new List<TextMeshProUGUI>();
            _fancyWeatherMonitorBGs = new List<Image>();
            _fancyWeatherMonitorTexts = new List<TextMeshProUGUI>();
            _salesMonitorBGs = new List<Image>();
            _salesMonitorTexts = new List<TextMeshProUGUI>();
            _extraBackgrounds = new List<Image>();

            // Resize the two extra monitor texts to be the same as their respective backgrounds, and give them padding
            _originalProfitQuotaBG = StartOfRound.Instance.profitQuotaMonitorBGImage;
            _originalProfitQuotaBG.color = Plugin.MonitorBackgroundColorVal;
            _originalProfitQuotaText = StartOfRound.Instance.profitQuotaMonitorText;
            _originalProfitQuotaText.color = Plugin.MonitorTextColorVal;
            _originalDeadlineBG = StartOfRound.Instance.deadlineMonitorBGImage;
            _originalDeadlineBG.color = Plugin.MonitorBackgroundColorVal;
            _originalDeadlineText = StartOfRound.Instance.deadlineMonitorText;
            _originalDeadlineText.color = Plugin.MonitorTextColorVal;
            _originalProfitQuotaText.rectTransform.sizeDelta = _originalProfitQuotaBG.rectTransform.sizeDelta;
            _originalProfitQuotaText.transform.position = _originalProfitQuotaBG.transform.TransformPoint(Vector3.back);
            _originalProfitQuotaText.fontSize = _originalProfitQuotaText.fontSize * 0.9f;
            _originalProfitQuotaText.margin = Vector4.one * 5;
            _originalDeadlineText.rectTransform.sizeDelta = _originalDeadlineBG.rectTransform.sizeDelta;
            _originalDeadlineText.transform.position = _originalDeadlineBG.transform.TransformPoint(Vector3.back);
            _originalDeadlineText.fontSize = _originalDeadlineText.fontSize * 0.9f;
            _originalDeadlineText.margin = Vector4.one * 5;

            if (Plugin.CenterAlignMonitorText.Value)
            {
                _originalProfitQuotaText.alignment = TextAlignmentOptions.Center;
                _originalDeadlineText.alignment = TextAlignmentOptions.Center;
            }

            // Find our monitor objects
            _UIContainer = StartOfRound.Instance.profitQuotaMonitorBGImage.transform.parent;
            _oldMonitorsObject = _UIContainer.parent.parent;
            _oldBigMonitors = _oldMonitorsObject.parent.GetComponentInChildren<ManualCameraRenderer>().transform.parent;

            // Increase internal ship cam resolution and FPS if specified
            var internalShipCamObj = StartOfRound.Instance.elevatorTransform.Find("Cameras/ShipCamera");
            var newRT = UpdateSecurityCamFPSAndResolution(internalShipCamObj, Plugin.ShipInternalCamFPS.Value, Plugin.ShipInternalCamSizeMultiplier.Value);
            _oldMonitorsObject.GetComponent<MeshRenderer>().sharedMaterials[2].mainTexture = newRT;

            // Increase external ship cam resolution and FPS if specified
            var externalShipCamObj = StartOfRound.Instance.elevatorTransform.Find("Cameras/FrontDoorSecurityCam/SecurityCamera");
            newRT = UpdateSecurityCamFPSAndResolution(externalShipCamObj, Plugin.ShipExternalCamFPS.Value, Plugin.ShipExternalCamSizeMultiplier.Value);
            _oldBigMonitors.GetComponent<MeshRenderer>().sharedMaterials[2].mainTexture = newRT;

            if (Plugin.UseBetterMonitors.Value)
            {
                CreateNewStyleMonitors();
            }
            else
            {
                CreateOldStyleMonitors();
            }

            // Remove or update profit quota scan node
            _profitQuotaScanNode = StartOfRound.Instance.elevatorTransform.GetComponentsInChildren<ScanNodeProperties>().FirstOrDefault(s => s.headerText == "Quota");
            if (_profitQuotaScanNode != null)
            {
                // Remove scan node
                if (!MonitorNames.MonitorExists(MonitorNames.ProfitQuota))
                {
                    Object.Destroy(_profitQuotaScanNode.gameObject);
                }
                else if (!Plugin.UseBetterMonitors.Value && _profitQuotaTexts.Any())
                {
                    // Update scan node position immediately (better monitors do it delayed)
                    _profitQuotaScanNode.transform.parent = _profitQuotaTexts[0].transform;
                    _profitQuotaScanNode.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                }
            }
        }

        private static RenderTexture UpdateSecurityCamFPSAndResolution(Transform cam, int fps, int resMultiplier)
        {
            var shipCam = cam.GetComponent<Camera>();
            var camRenderer = cam.GetComponent<ManualCameraRenderer>();

            camRenderer.renderAtLowerFramerate = fps > 0;
            camRenderer.fps = fps;
            if (resMultiplier > 1)
            {
                var newCamRT = new RenderTexture(shipCam.targetTexture);
                for (int i = 0; i < resMultiplier; i++)
                {
                    newCamRT.width *= 2;
                    newCamRT.height *= 2;
                }
                shipCam.targetTexture = newCamRT;
            }

            return shipCam.targetTexture;
        }

        private static void CreateOldStyleMonitors()
        {
            // Copy everything from the existing quota monitor
            _originalProfitQuotaBG.gameObject.SetActive(false);
            _originalProfitQuotaText.gameObject.SetActive(false);
            _originalDeadlineBG.gameObject.SetActive(false);
            _originalDeadlineText.gameObject.SetActive(false);
            _originalDeadlineBG.transform.localPosition = _originalProfitQuotaBG.transform.localPosition;
            _originalDeadlineBG.transform.localPosition = _originalProfitQuotaBG.transform.localPosition;
            _originalDeadlineText.transform.localPosition = _originalProfitQuotaText.transform.localPosition;
            _originalDeadlineText.transform.localRotation = _originalProfitQuotaText.transform.localRotation;

            // Store positions and rotations by offset based on monitor index
            var originalPos = _originalProfitQuotaBG.transform.localPosition;
            var originalRot = _originalProfitQuotaBG.transform.localEulerAngles;
            var offsets = new List<KeyValuePair<Vector3, Vector3>>
            {
                new KeyValuePair<Vector3, Vector3>(new Vector3(0, 465, -22), new Vector3(-18, 0, 0)),       // Monitor 1
                new KeyValuePair<Vector3, Vector3>(new Vector3(470, 465, -22), new Vector3(-18, 0, 0)),     // Monitor 2
                new KeyValuePair<Vector3, Vector3>(new Vector3(970, 485, -128), new Vector3(-18, 25, 5)),   // Monitor 3
                new KeyValuePair<Vector3, Vector3>(new Vector3(1390, 525, -329), new Vector3(-18, 25, 5)),  // Monitor 4
                new KeyValuePair<Vector3, Vector3>(Vector3.zero, Vector3.zero),                             // Monitor 5
                new KeyValuePair<Vector3, Vector3>(new Vector3(470, 0, 0), Vector3.zero),                   // Monitor 6
                new KeyValuePair<Vector3, Vector3>(new Vector3(1025, 30, -115), new Vector3(-1, 25, 5)),    // Monitor 7
                new KeyValuePair<Vector3, Vector3>(new Vector3(1445, 72, -320), new Vector3(-1, 27, 5))     // Monitor 8
            };

            // Assign monitors to the positions that were specified, ensuring to not overlap
            for (int i = 0; i < offsets.Count; i++)
            {
                string curAssignment = Plugin.ShipMonitorAssignments[i].Value;
                List<Image> curBGs = null;
                List<TextMeshProUGUI> curTexts = null;

                switch (curAssignment)
                {
                    case MonitorNames.ProfitQuota: curBGs = _profitQuotaBGs; curTexts = _profitQuotaTexts; break;
                    case MonitorNames.Deadline: curBGs = _deadlineBGs; curTexts = _deadlineTexts; break;
                    case MonitorNames.ShipScrap: curBGs = _shipScrapMonitorBGs; curTexts = _shipScrapMonitorTexts; break;
                    case MonitorNames.Time: curBGs = _timeMonitorBGs; curTexts = _timeMonitorTexts; break;
                    case MonitorNames.Weather: curBGs = _weatherMonitorBGs; curTexts = _weatherMonitorTexts; break;
                    case MonitorNames.FancyWeather: curBGs = _fancyWeatherMonitorBGs; curTexts = _fancyWeatherMonitorTexts; break;
                    case MonitorNames.Sales: curBGs = _salesMonitorBGs; curTexts = _salesMonitorTexts; break;
                }

                if ((curBGs == null || curTexts == null) && Plugin.ShowBlueMonitorBackground.Value && Plugin.ShowBackgroundOnAllScreens.Value)
                {
                    // Prepare to create a blank background
                    curBGs = _extraBackgrounds;
                }
                else if (curTexts == null && !string.IsNullOrWhiteSpace(curAssignment))
                {
                    Plugin.MLS.LogError($"Could not find '{curAssignment}' for monitor assignment! Please check your config is using acceptable values.");
                }

                var positionOffset = offsets[i].Key;
                var rotationOffset = offsets[i].Value;

                // Only create a background if we have one assigned, or we want to show extra backgrounds
                if (curBGs != null && (curBGs != _extraBackgrounds || Plugin.ShowBlueMonitorBackground.Value))
                {
                    var newBG = Object.Instantiate(_originalProfitQuotaBG, _originalProfitQuotaBG.transform.parent);
                    newBG.gameObject.SetActive(true);
                    newBG.name = $"{(string.IsNullOrWhiteSpace(curAssignment) ? "ExtraBackground" : curAssignment)}BG{i + 1}";

                    newBG.transform.localPosition = originalPos + positionOffset;
                    newBG.transform.localEulerAngles = originalRot + rotationOffset;
                    curBGs.Add(newBG);
                }

                // Text will be null if this is a blank background
                if (curTexts != null)
                {
                    Plugin.MLS.LogInfo($"Creating {curAssignment} monitor at position {i + 1}");
                    var newText = Object.Instantiate(_originalProfitQuotaText, _originalProfitQuotaText.transform.parent);
                    newText.gameObject.SetActive(true);
                    newText.name = $"{curAssignment}Text{i + 1}";

                    newText.transform.localPosition = originalPos + positionOffset + new Vector3(0, 0, -1);
                    newText.transform.localEulerAngles = originalRot + rotationOffset;
                    curTexts.Add(newText);
                }
            }

            foreach (var fancyWeather in _fancyWeatherMonitorTexts)
            {
                fancyWeather.alignment = TextAlignmentOptions.MidlineLeft;
                fancyWeather.transform.localPosition += new Vector3(25, 0, -10);
                fancyWeather.transform.localEulerAngles += new Vector3(-2, 1, 0);
            }

            // Initialize everything's text
            CopyProfitQuotaAndDeadlineTexts();
            UpdateShipScrapMonitors();
            UpdateTimeMonitors();
            UpdateWeatherMonitors();
            UpdateSalesMonitors();
        }

        private static void CreateNewStyleMonitors()
        {
            Plugin.MLS.LogInfo("Overwriting monitors with new model");

            var newMonitorsObj = Object.Instantiate(AssetBundleHelper.MonitorsPrefab, _oldMonitorsObject.transform.parent);
            newMonitorsObj.transform.SetLocalPositionAndRotation(_oldMonitorsObject.localPosition, Quaternion.identity);

            _newMonitors = newMonitorsObj.AddComponent<Monitors>();
            _newMonitors.StartingMapMaterial = _oldBigMonitors.GetComponent<MeshRenderer>().sharedMaterials[1];
            _newMonitors.HullMaterial = _oldMonitorsObject.GetComponent<MeshRenderer>().materials[0];
            _newMonitors.BlankScreenMat = _oldMonitorsObject.GetComponent<MeshRenderer>().materials[1];

            // Assign specified TMP objects to the monitor indexes specified
            for (int i = 0; i < Plugin.ShipMonitorAssignments.Length; i++)
            {
                string curAssignment = Plugin.ShipMonitorAssignments[i].Value;
                Action<TextMeshProUGUI> curAction = null;
                Material targetMat = null;

                switch (curAssignment)
                {
                    case MonitorNames.ProfitQuota: curAction = t => { _profitQuotaTexts.Add(t); CopyProfitQuotaAndDeadlineTexts(); }; break;
                    case MonitorNames.Deadline: curAction = t => { _deadlineTexts.Add(t); CopyProfitQuotaAndDeadlineTexts(); }; break;
                    case MonitorNames.ShipScrap: curAction = t => { _shipScrapMonitorTexts.Add(t); UpdateShipScrapMonitors(); }; break;
                    case MonitorNames.Time: curAction = t => { _timeMonitorTexts.Add(t); UpdateTimeMonitors(); }; break;
                    case MonitorNames.Weather: curAction = t => { _weatherMonitorTexts.Add(t); UpdateWeatherMonitors(); }; break;
                    case MonitorNames.FancyWeather:
                        curAction = t =>
                    {
                        t.alignment = TextAlignmentOptions.MidlineLeft;
                        t.margin += new Vector4(25, 0, 0, 0);
                        _fancyWeatherMonitorTexts.Add(t);
                        UpdateWeatherMonitors();
                    };
                        break;
                    case MonitorNames.Sales: curAction = t => { _salesMonitorTexts.Add(t); UpdateSalesMonitors(); }; break;

                    case MonitorNames.InternalCam: targetMat = _oldMonitorsObject.GetComponent<MeshRenderer>().materials[2]; break;
                    case MonitorNames.ExternalCam: targetMat = _oldBigMonitors.GetComponent<MeshRenderer>().materials[2]; break;
                }

                if (curAction != null || targetMat != null)
                {
                    Plugin.MLS.LogInfo($"Creating {curAssignment} monitor at position {i + 1}");
                    if (curAction != null)
                    {
                        _newMonitors.AssignTextMonitor(i, curAction);
                    }
                    else
                    {
                        _newMonitors.AssignMaterialMonitor(i, targetMat);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(curAssignment))
                {
                    Plugin.MLS.LogError($"Could not find '{curAssignment}' for monitor assignment! Please check your config is using acceptable values.");
                }
            }

            var newMesh = newMonitorsObj.transform.Find("Monitors/BigMiddle").GetComponent<MeshRenderer>();
            StartOfRound.Instance.mapScreen.mesh = newMesh;
            StartOfRound.Instance.elevatorTransform.Find("Cameras/ShipCamera").GetComponent<ManualCameraRenderer>().mesh = newMesh;
            StartOfRound.Instance.elevatorTransform.Find("Cameras/FrontDoorSecurityCam/SecurityCamera").GetComponent<ManualCameraRenderer>().mesh = newMesh;
        }

        public static void HideOldMonitors()
        {
            Plugin.MLS.LogInfo("Hiding old monitors");

            if (_oldBigMonitors != null && _oldMonitorsObject != null && _UIContainer != null)
            {
                // Hide the old monitor objects and update the ManualCameraRender meshes to null so the cams do not get disabled
                _oldBigMonitors.GetComponent<MeshRenderer>().enabled = false;
                _oldBigMonitors.GetComponent<Collider>().enabled = false;
                _oldMonitorsObject.GetComponent<MeshRenderer>().enabled = false;
                _oldMonitorsObject.GetComponent<Collider>().enabled = false;
                for (int i = 0; i < _UIContainer.childCount; i++)
                {
                    if (_UIContainer.GetChild(i).GetComponent<Image>() is Image img)
                    {
                        img.enabled = false;
                    }
                    else if (_UIContainer.GetChild(i).GetComponent<TextMeshProUGUI>() is TextMeshProUGUI txt)
                    {
                        txt.enabled = false;
                    }
                }
            }
        }

        public static void CopyProfitQuotaAndDeadlineTexts()
        {
            UpdateGenericTextList(_profitQuotaTexts, StartOfRound.Instance.profitQuotaMonitorText?.text);
            UpdateGenericTextList(_deadlineTexts, StartOfRound.Instance.deadlineMonitorText?.text);
        }

        public static void UpdateShipScrapMonitors()
        {
            if (!_shipScrapMonitorTexts.Any())
            {
                return;
            }

            var allScrap = Object.FindObjectsOfType<GrabbableObject>().Where(o => o.itemProperties.isScrap && o.isInShipRoom && o.isInElevator && !o.isHeld).ToList();
            int shipLoot = allScrap.Sum(o => o.scrapValue);

            UpdateGenericTextList(_shipScrapMonitorTexts, $"SCRAP IN SHIP:\n${shipLoot}");
            Plugin.MLS.LogInfo($"Set ship scrap total to ${shipLoot} ({allScrap.Count} items).");
        }

        public static void UpdateTimeMonitors()
        {
            if (HUDManager.Instance?.clockNumber != null && _timeMonitorTexts.Any())
            {
                Plugin.MLS.LogDebug("Updating time display.");
                string time;
                if (TimeOfDay.Instance.movingGlobalTimeForward)
                {
                    time = $"TIME:\n{HUDManager.Instance.clockNumber.text.Replace('\n', ' ')}";
                }
                else
                {
                    time = "TIME:\nPENDING";
                }
                UpdateGenericTextList(_timeMonitorTexts, time);
            }
        }

        public static void UpdateWeatherMonitors()
        {
            if (_weatherMonitorTexts.Any() || _fancyWeatherMonitorTexts.Any())
            {
                Plugin.MLS.LogInfo("Updating weather monitor");

                if (_weatherMonitorTexts.Any())
                {
                    UpdateGenericTextList(_weatherMonitorTexts, $"WEATHER:\n{(StartOfRound.Instance.currentLevel?.currentWeather.ToString() ?? string.Empty)}");
                }

                if (_fancyWeatherMonitorTexts.Any())
                {
                    // Change the animation we are currently referencing
                    _curWeatherAnimations = StartOfRound.Instance.currentLevel?.currentWeather switch
                    {
                        LevelWeatherType.None => WeatherASCIIArt.ClearAnimations,
                        LevelWeatherType.Rainy => WeatherASCIIArt.RainAnimations,
                        LevelWeatherType.Stormy => WeatherASCIIArt.RainAnimations,
                        LevelWeatherType.Foggy => WeatherASCIIArt.FoggyAnimations,
                        LevelWeatherType.Flooded => WeatherASCIIArt.FloodedAnimations,
                        LevelWeatherType.Eclipsed => WeatherASCIIArt.EclipsedAnimations,
                        _ => new string[] { string.Empty }
                    };

                    _hasOverlays = StartOfRound.Instance.currentLevel?.currentWeather == LevelWeatherType.Stormy;
                    if (_hasOverlays)
                    {
                        _overlayTimer = 0;
                        _overlayCycle = Random.Range(0.1f, 3);
                        _curWeatherOverlays = WeatherASCIIArt.LightningOverlays;
                        _curWeatherOverlayIndex = Random.Range(0, _curWeatherOverlays.Length);
                    }
                    _showingOverlay = false;

                    _curWeatherAnimIndex = 0;
                    _animTimer = 0;
                    UpdateGenericTextList(_fancyWeatherMonitorTexts, _curWeatherAnimations[_curWeatherAnimIndex]);
                }
            }
        }

        public static void AnimateWeatherMonitors()
        {
            if (!_fancyWeatherMonitorTexts.Any() || _curWeatherAnimations.Length < 2)
            {
                return;
            }

            Action drawWeather = () =>
            {
                var sb = new StringBuilder();
                string[] animLines = _curWeatherAnimations[_curWeatherAnimIndex].Split(Environment.NewLine);
                string[] overlayLines = (_showingOverlay ? _curWeatherOverlays[_curWeatherOverlayIndex] : string.Empty).Split(Environment.NewLine);

                // Loop through each line of the current animation frame, overwriting any characters with a matching overlay character if one exists
                for (int l = 0; l < animLines.Length; l++)
                {
                    string curAnimLine = animLines[l];
                    string overlayLine = overlayLines.ElementAtOrDefault(l);

                    for (int c = 0; c < curAnimLine.Length; c++)
                    {
                        bool isOverlayChar = !string.IsNullOrWhiteSpace(overlayLine) && overlayLine.Length > c && overlayLine[c] != ' ';
                        sb.Append(isOverlayChar ? $"<color=#ffe100>{overlayLine[c]}</color>" : $"{curAnimLine[c]}");
                    }
                    sb.AppendLine();
                }

                UpdateGenericTextList(_fancyWeatherMonitorTexts, sb.ToString());
            };

            // Cycle through our current animation pattern 'sprites'
            _animTimer += Time.deltaTime;
            if (_animTimer >= _animCycle)
            {
                _curWeatherAnimIndex = (_curWeatherAnimIndex + 1) % _curWeatherAnimations.Length;
                _animTimer = 0;
                drawWeather();
            }

            // Handle random overlays
            if (_hasOverlays)
            {
                _overlayTimer += Time.deltaTime;
                if (_overlayTimer >= (_showingOverlay ? 0.5f : _overlayCycle))
                {
                    _overlayTimer = 0;
                    _showingOverlay = !_showingOverlay;

                    if (!_showingOverlay)
                    {
                        // Reset the counter for the next overlay
                        _overlayCycle = Random.Range(0.1f, 3);
                        _curWeatherOverlayIndex = Random.Range(0, _curWeatherOverlays.Length);
                    }

                    drawWeather();
                }
            }
        }

        public static void UpdateSalesMonitors()
        {
            if (MonitorNames.MonitorExists(MonitorNames.Sales) && _salesMonitorTexts != null)
            {
                Plugin.MLS.LogDebug("Updating sales display.");
                UpdateGenericTextList(_salesMonitorTexts, "SALES COMING SOON!");
            }
        }

        private static void UpdateGenericTextList(List<TextMeshProUGUI> textList, string text)
        {
            foreach (var t in textList)
            {
                t.text = text;
                if (_newMonitors != null)
                {
                    _newMonitors.RenderCameraAfterTextChange(t);
                }
            }
        }

        public static void ToggleExtraMonitorsPower(bool on)
        {
            if (Plugin.SyncExtraMonitorsPower.Value)
            {
                if (Plugin.UseBetterMonitors.Value)
                {
                    if (_newMonitors != null)
                    {
                        _newMonitors.TogglePower(on);
                    }
                }
                else
                {
                    if (Plugin.ShowBlueMonitorBackground.Value)
                    {
                        foreach (var background in _profitQuotaBGs.Concat(_deadlineBGs).Concat(_shipScrapMonitorBGs).Concat(_timeMonitorBGs)
                            .Concat(_weatherMonitorBGs).Concat(_fancyWeatherMonitorBGs).Concat(_salesMonitorBGs).Concat(_extraBackgrounds)
                            .Where(b => b != null))
                        {
                            background.gameObject.SetActive(on);
                        }
                    }

                    foreach (var text in _profitQuotaTexts.Concat(_deadlineTexts).Concat(_shipScrapMonitorTexts).Concat(_timeMonitorTexts)
                        .Concat(_weatherMonitorTexts).Concat(_fancyWeatherMonitorTexts).Concat(_salesMonitorTexts)
                        .Where(b => b != null))
                    {
                        text.gameObject.SetActive(on);
                    }
                }
            }
        }

        public static void UpdateMapMaterial(Material newMaterial)
        {
            if (_newMonitors != null)
            {
                _newMonitors.UpdateMapMaterial(newMaterial);
            }
        }
    }
}