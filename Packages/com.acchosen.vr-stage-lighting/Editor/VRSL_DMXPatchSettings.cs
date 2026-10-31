#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Debug = UnityEngine.Debug;

#if UDONSHARP
using VRC.Udon;
using UdonSharpEditor;
#endif

#if !UNITY_EDITOR_LINUX && !UNITY_ANDROID && !UNITY_IOS
using System.Drawing.Printing;
#endif

namespace VRSL.EditorScripts
{
    [CreateAssetMenu(menuName = "VRSL/DMX Fixture Patch File", fileName = "VRSL DMX Fixture Patch File")]
    [Serializable]
    public class VRSL_DMXPatchSettings : ScriptableObject
    {
        private const string Prefix = "VRSL DMX Patch Settings";
        
        [HideInInspector]
        public Scene targetScene;
        [HideInInspector] 
        public string scenePath;
        [HideInInspector]
        public string[] idStrings;
        [HideInInspector]
        public DMXFixtureData[] data;

        private VRSL_LocalUIControlPanel _panel;
        private string[] _dataToPdf;
        private int _pdfLineCount;
        private int _pdfPageCount = 1;
        private DataTable _patchList;
        
        public void CheckData()
        {
            if (data == null)
            {
                Debug.Log("DMX Fixture Data Array is null!");
            }
            else
            {
                if (data.Length == 0)
                {
                    Debug.Log("DMX Fixture Data Array is length 0!");
                }
            }
        }
        
        public void ForceSave()
        {
            var assetPath =  AssetDatabase.GetAssetPath(GetInstanceID());
            
            AssetDatabase.RenameAsset(assetPath, "VRSL DMX Patch Data_" + targetScene.name);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<VRSL_DMXPatchSettings>(AssetDatabase.GetAssetPath(GetInstanceID())); 
        }
        
        public void SetScene()
        {
            targetScene = SceneManager.GetActiveScene();
            scenePath = targetScene.path;
        }
        
        public void SetDmxFixtureData()
        {
            Debug.Log($"{Prefix}: Saving Fixture Data...");
            var sceneObjects = GetAllObjectsOnlyInScene();
            var sceneFixtures = new List<VRStageLighting_DMX_Static>();
            foreach(var go in sceneObjects)
            {
#if UDONSHARP
                #pragma warning disable 0618 //suppressing obsoletion warnings
                var lightScript = go.GetUdonSharpComponent<VRStageLighting_DMX_Static>();
                #pragma warning restore 0618 //suppressing obsoletion warnings
#else
                VRStageLighting_DMX_Static lightScript = go.GetComponent<VRStageLighting_DMX_Static>();
#endif
                if (lightScript != null)
                {
                    sceneFixtures.Add(lightScript);
                }
            }
            
            var fixtures = sceneFixtures.ToArray();
            if (fixtures.Length > 0)
            {
                targetScene = SceneManager.GetActiveScene();
                scenePath = targetScene.path;

                data = new DMXFixtureData[fixtures.Length];
                var ids = new GlobalObjectId[fixtures.Length];
                idStrings = new string[fixtures.Length];
                GlobalObjectId.GetGlobalObjectIdsSlow(fixtures, ids);
                for(var i = 0; i < data.Length; i++)
                {
                    idStrings[i] = ids[i].ToString();
                    data[i] = new DMXFixtureData(fixtures[i], ids[i]);
                }
            }
        }
        
        private static List<GameObject> GetAllObjectsOnlyInScene()
        {
            var objectsInScene = new List<GameObject>();

            foreach (var go in (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)))
            {
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                {
                    objectsInScene.Add(go);
                }
            }

            return objectsInScene;
        }

        public void LoadDmxFixtureData()
        {
            if (data == null)
            {
                return;
            }

            if (data.Length <= 0)
            {
                return;
            }
            
            var sceneObjects = GetAllObjectsOnlyInScene();
            var sceneFixtures = new List<VRStageLighting_DMX_Static>();
            foreach(var go in sceneObjects)
            {
#if UDONSHARP
#pragma warning disable 0618 //suppressing obsoletion warnings
                VRStageLighting_DMX_Static lightScript = go.GetUdonSharpComponent<VRStageLighting_DMX_Static>();
#pragma warning restore 0618 //suppressing obsoletion warnings
#else
                    VRStageLighting_DMX_Static lightScript = go.GetComponent<VRStageLighting_DMX_Static>();
#endif
                if (lightScript != null)
                {
                    sceneFixtures.Add(lightScript);
                }
            }

            foreach(var fixture in sceneFixtures)
            {
                try
                {
                    var success = false;
                    var dmxID = 0;
                    var id = GlobalObjectId.GetGlobalObjectIdSlow(fixture);
                    var guid = id.assetGUID.ToString();
                        
                    for(var i = 0; i < data.Length; i++)
                    {
                        ulong localID;
                        if (PrefabUtility.IsPartOfAnyPrefab(fixture))
                        {
                            localID = id.targetPrefabId;
                            if (guid == data[i].assetGUID)
                            {
                                if (localID == data[i].targetPrefabId)
                                {
                                    Debug.Log($"{Prefix}: Found {localID} successfully at {fixture.name}");
                                    success = true;
                                    dmxID = i;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            localID = id.targetObjectId;    
                            if (guid == data[i].assetGUID)
                            {
                                if (localID == data[i].targetObjectId)
                                {
                                    Debug.Log($"{Prefix}: Found {localID} successfully at {fixture.name}");
                                    success = true;
                                    dmxID = i;
                                    break;
                                }
                            }
                        }
                    }
                    if (success)
                    {
                        var so = new SerializedObject(fixture);
                        so.FindProperty(nameof(fixture.enableDMXChannels)).boolValue = data[dmxID].enableDMXChannels;
                        so.FindProperty(nameof(fixture.fixtureID)).intValue = data[dmxID].fixtureID;
                        so.FindProperty(nameof(fixture.dmxChannel)).intValue = data[dmxID].dmxChannel;
                        so.FindProperty(nameof(fixture.dmxUniverse)).intValue = data[dmxID].dmxUniverse;
                        so.FindProperty(nameof(fixture.Channel)).intValue = data[dmxID].Channel;
                        so.FindProperty(nameof(fixture.legacyGoboRange)).boolValue = data[dmxID].legacyGoboRange;
                        so.FindProperty(nameof(fixture.globalIntensity)).floatValue = data[dmxID].globalIntensity;
                        so.FindProperty(nameof(fixture.finalIntensity)).floatValue = data[dmxID].finalIntensity;
                        so.FindProperty(nameof(fixture.lightColorTint)).colorValue = data[dmxID].lightColorTint;
                        so.FindProperty(nameof(fixture.invertPan)).boolValue = data[dmxID].invertPan;
                        so.FindProperty(nameof(fixture.invertTilt)).boolValue = data[dmxID].invertTilt;
                        so.FindProperty(nameof(fixture.isUpsideDown)).boolValue = data[dmxID].isUpsideDown;
                        so.FindProperty(nameof(fixture.enableAutoSpin)).boolValue = data[dmxID].enableAutoSpin;
                        so.FindProperty(nameof(fixture.enableStrobe)).boolValue = data[dmxID].enableStrobe;
                        so.FindProperty(nameof(fixture.tiltOffsetBlue)).floatValue = data[dmxID].tiltOffsetBlue;
                        so.FindProperty(nameof(fixture.panOffsetBlueGreen)).floatValue = data[dmxID].panOffsetBlueGreen;
                        so.FindProperty(nameof(fixture.selectGOBO)).intValue = data[dmxID].selectGOBO;
                        so.FindProperty(nameof(fixture.coneWidth)).floatValue = data[dmxID].coneWidth;
                        so.FindProperty(nameof(fixture.coneLength)).floatValue = data[dmxID].coneLength;
                        so.FindProperty(nameof(fixture.maxConeLength)).floatValue = data[dmxID].maxConeLength;
                        so.FindProperty(nameof(fixture.maxMinPan)).floatValue = data[dmxID].maxMinPan;
                        so.FindProperty(nameof(fixture.maxMinTilt)).floatValue = data[dmxID].maxMinTilt;
                        so.FindProperty(nameof(fixture.fixtureDefintion)).intValue = data[dmxID].fixtureDefintion;
                        var rendsProp = so.FindProperty(nameof(fixture.objRenderers));
                        rendsProp.ClearArray();
                        so.ApplyModifiedProperties();

                        var sof = new SerializedObject(fixture);
                        var rendsProperty = sof.FindProperty(nameof(fixture.objRenderers));
                        var rends = data[dmxID].objRenderers.GetRenderers();
                        rendsProperty.arraySize = rends.Length;
                        for(int i = 0; i < rends.Length; i++)
                        {
                            //Debug.Log("Array Element: " + i);
                            rendsProperty.InsertArrayElementAtIndex(i);
                            rendsProperty.GetArrayElementAtIndex(i).objectReferenceValue = rends[i];
                        }
                        sof.ApplyModifiedProperties();

#if UDONSHARP
#pragma warning disable 0618 //suppressing obsoletion warnings
                        fixture.UpdateProxy();
#pragma warning restore 0618 //suppressing obsoletion warnings.
#endif

                        // TODO: replace with internal copy method
                        fixture.enableDMXChannels = data[dmxID].enableDMXChannels;
                        fixture.fixtureID = data[dmxID].fixtureID;
                        fixture.dmxChannel = data[dmxID].dmxChannel;
                        fixture.dmxUniverse = data[dmxID].dmxUniverse;
                        fixture.Channel = data[dmxID].Channel;
                        fixture.legacyGoboRange = data[dmxID].legacyGoboRange;
                        fixture.globalIntensity = data[dmxID].globalIntensity;
                        fixture.finalIntensity = data[dmxID].finalIntensity;
                        fixture.lightColorTint = data[dmxID].lightColorTint;
                        fixture.invertPan = data[dmxID].invertPan;
                        fixture.invertTilt = data[dmxID].invertTilt;
                        fixture.isUpsideDown = data[dmxID].isUpsideDown;
                        fixture.enableAutoSpin = data[dmxID].enableAutoSpin;
                        fixture.enableStrobe = data[dmxID].enableStrobe;
                        fixture.tiltOffsetBlue = data[dmxID].tiltOffsetBlue;
                        fixture.panOffsetBlueGreen = data[dmxID].panOffsetBlueGreen;
                        fixture.selectGOBO = data[dmxID].selectGOBO;
                        fixture.coneWidth = data[dmxID].coneWidth;
                        fixture.coneLength = data[dmxID].coneLength;
                        fixture.maxConeLength =  data[dmxID].maxConeLength;
                        fixture.maxMinPan = data[dmxID].maxMinPan;
                        fixture.maxMinTilt = data[dmxID].maxMinTilt;
                        fixture.fixtureDefintion = data[dmxID].fixtureDefintion;
                        fixture.objRenderers = rends;

#if UDONSHARP
#pragma warning disable 0618 //suppressing obsoletion warnings
                        fixture.ApplyProxyModifications();
#pragma warning restore 0618 //suppressing obsoletion warnings
#endif
                        if (PrefabUtility.IsPartOfAnyPrefab(fixture))
                        {
                            PrefabUtility.RecordPrefabInstancePropertyModifications(fixture);
                        }
                        if (Application.isPlaying)
                        {
                            fixture._UpdateInstancedProperties();
                        }
                        else
                        {
                            fixture._UpdateInstancedPropertiesSansDMX();
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
        
        private bool CheckForLocalPanel()
        {
            var sceneObjects = GetAllObjectsOnlyInScene();
            var hasLocalPanel = false;
            _panel = null;
            
            foreach (var go in sceneObjects)
            {
#if UDONSHARP
                #pragma warning disable 0618 //suppressing obsoletion warnings
                _panel = go.GetUdonSharpComponent<VRSL_LocalUIControlPanel>();
                #pragma warning restore 0618
#else
                _panel = go.GetComponent<VRSL_LocalUIControlPanel>();
#endif
                if (_panel != null)
                {
                    hasLocalPanel = true;
                    break;
                }
            }
            return hasLocalPanel;
        }
        
#if !UNITY_EDITOR_LINUX && !UNITY_ANDROID && !UNITY_IOS
        private void OnPrintPage(object sender, PrintPageEventArgs ev)
        {   
            _pdfPageCount++;
            ev.HasMorePages = DrawTable(ev.Graphics);          
        }

        private bool DrawTable(System.Drawing.Graphics graph)
        {
            var rowHeight = 30;
            var rowCount = 33;
            var tableHeight = rowHeight * rowCount;
            var numOfItems = Mathf.Abs(data.Length - _pdfLineCount);
            if(numOfItems < rowCount)
            {
                rowCount = numOfItems;
            }
            var image = new Bitmap(800, (rowHeight * rowCount)-10);
            var xOffset = 25;
            var yOFfset = 50;
            try 
            {
                graph.DrawString(name, new System.Drawing.Font("Arial", 16), Brushes.Black,new Point(xOffset,15));
                graph.FillRectangle(Brushes.White, new Rectangle(new Point(0 + xOffset, 0+ yOFfset), image.Size));
                var col = 0 + xOffset;
                var finalNumberofRows = 0;
                var drewRectangle = false;
                
                for (int i = 0; i < 5; i++) 
                {
                    var nextColumn = 0;
                    string columnName;
                    graph.DrawLine(Pens.Black, new Point(col, 0 + yOFfset), new Point(col, image.Height + yOFfset));
                    
                    switch(i)
                    {
                        default:
                            nextColumn += 50;
                            columnName = "ID";
                            break;
                        case 1:
                            nextColumn += 400;
                            columnName = "Fixture Name";
                            break;
                        case 2:
                            nextColumn += 200;
                            columnName = "Fixture Type";
                            break;
                        case 3:
                            nextColumn += 75;
                            columnName = "Universe";
                            break;
                        case 4:
                            nextColumn += 75;
                            columnName = "Address";
                            break;
                    }
                    finalNumberofRows = 0 + _pdfLineCount;
                    
                    for (var row = 0 + yOFfset; row <= image.Height; row += rowHeight) 
                    {
                        if (row == 0 + yOFfset)
                        {
                            if (!drewRectangle)
                            {
                                graph.FillRectangle(Brushes.Gray, new Rectangle(new Point(col, row), new Size(image.Width,rowHeight)));
                                drewRectangle = true;
                            }

                            graph.DrawLine(Pens.Black, new Point(col, row), new Point(col + nextColumn, row));
                            graph.DrawString(columnName, new System.Drawing.Font("Arial", 12), Brushes.Black,new Point(col+1,row+8));
                        }
                        else
                        {
                            if (finalNumberofRows < _patchList.Rows.Count)
                            {
                                graph.DrawLine(Pens.Black, new Point(col, row), new Point(col + nextColumn, row));
                                graph.DrawString(_patchList.Rows[finalNumberofRows][i].ToString(), new System.Drawing.Font("Arial", 12), Brushes.Black,new Point(col+1,row+8));
                                finalNumberofRows++;
                            }
                        }
                    }
                    col+= nextColumn;
                }
                _pdfLineCount += finalNumberofRows;

                graph.DrawRectangle(Pens.Black, new Rectangle(0+ xOffset, 0 + yOFfset, image.Width - 1, image.Height - 1));
                graph.DrawString(_pdfPageCount.ToString(), new System.Drawing.Font("Arial", 12), Brushes.Black,new Point((image.Width / 2) + xOffset ,tableHeight + yOFfset + 20));
            } 
            finally 
            {
                image.Dispose();
            }
            
            return _pdfLineCount < data.ToArray().Length;
        }
        
        public void ToPdf()
        {
            if (!CheckForLocalPanel()) return;
            
            _pdfLineCount = 0;
            _pdfPageCount = 0;

            _patchList = new DataTable("Patch List");
            DataColumn column;
            DataRow row;
            
            //ID Column
            column = new DataColumn();
            column.DataType = typeof(int);
            column.ColumnName = "ID";
            column.Caption = "Fixture ID";
            column.ReadOnly = false;
            column.Unique = false;
            _patchList.Columns.Add(column);
            
            //Name Column
            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = "Fixture Name";
            column.Caption = "Fixture Name";
            column.ReadOnly = false;
            column.Unique = false;
            _patchList.Columns.Add(column);
            
            //Type Column
            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = "Fixture Type";
            column.Caption = "Fixture Type";
            column.ReadOnly = false;
            column.Unique = false;
            _patchList.Columns.Add(column);

            //Universe Column
            column = new DataColumn();
            column.DataType = typeof(int);
            column.ColumnName = "Universe";
            column.Caption = "Universe";
            column.ReadOnly = false;
            column.Unique = false;
            _patchList.Columns.Add(column);

            //Address Column
            column = new DataColumn();
            column.DataType = typeof(int);
            column.ColumnName = "Address";
            column.Caption = "Address";
            column.ReadOnly = false;
            column.Unique = false;
            _patchList.Columns.Add(column);

            var dtSet = new DataSet();

            dtSet.Tables.Add(_patchList);
            
            // TODO: replace with new fixture definitions
            var fixDefAsset = (VRSL_FixtureDefinitions) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_panel.fixtureDefGUID), typeof(VRSL_FixtureDefinitions));
            var definitions = fixDefAsset.GetNames();

            var sortedData = data.OrderBy(c => c.fixtureID).ToArray();
            
            foreach(var f in sortedData)
            {
                row = _patchList.NewRow();
                row["ID"] = f.fixtureID;
                row["Fixture Name"] = f.name;
                row["Fixture Type"] = definitions[f.fixtureDefintion];
                row["Universe"] = f.dmxUniverse;
                row["Address"] = f.dmxChannel;
                _patchList.Rows.Add(row);
            }

            _pdfLineCount = 0;
            var document = new PrintDocument();
            document.PrintPage += OnPrintPage;
            document.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            document.Print();
            Debug.Log($"{Prefix}: Successfully Exported PDF File"); 
        }
#endif
        
        public string ToJsonFile(bool refreshEditor)
        {
            if (!CheckForLocalPanel()) return "FAILED";
            
            try
            {
                var fixDefAsset = (VRSL_FixtureDefinitions) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_panel.fixtureDefGUID), typeof(VRSL_FixtureDefinitions));
                var jsonFixtureData = new JSONDMXFixtureData[data.Length];
                var definitions = fixDefAsset.GetNames();
                for(var i = 0; i < jsonFixtureData.Length; i++)
                {
                    jsonFixtureData[i] = new JSONDMXFixtureData(data[i], definitions, fixDefAsset.GetChannelDefinition(data[i].fixtureDefintion));
                }
                var jsonContainer = new JSONDMXFixtureData_Container(jsonFixtureData);
                var json = JsonUtility.ToJson(jsonContainer, true);
                var assetPath = AssetDatabase.GetAssetPath(this);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
                var targetString = name + ".asset";
                var newString = name + ".json";
                filePath = filePath.Replace("/", "\\");
                filePath = filePath.Replace(targetString, newString);
                File.WriteAllText(filePath, json);
                
                if (refreshEditor)
                {
                    AssetDatabase.Refresh();
                }
                Debug.Log($"{Prefix}: Successfully Exported JSON File");
                return filePath;
            }
            catch
            {
                Debug.LogError($"{Prefix}: Failed to Export to JSON!");
                return "FAILED";
            }
        }
        
        private string ToXMLTransformMatrix(Vector3 position, Vector3 rotation)
        {
            var rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(rotation));
            var u = rotationMatrix.GetRow(0);
            var v = rotationMatrix.GetRow(1);
            var w = rotationMatrix.GetRow(2);
            //Vector4 o = matrix.GetRow(3);
            var u1 = new Vector3(u.x, u.y, u.z);
            var v1 = new Vector3(v.x, v.y, v.z);
            var w1 = new Vector3(w.x, w.y, w.z);
            position = new Vector3(position.x * 1000f, position.y * 1000f, position.z * 1000f);
            //Vector3 o1 = new Vector3(o.x, o.y, o.z);
            var output = u1.ToString("F6") + " " + v1.ToString("F6") + " " + w1.ToString("F6") + " " + position.ToString("F6");
            output = output.Replace("(", "{");
            output = output.Replace(")", "}");
            output = output.Replace(" ", "");
            return output;
        }

        private int GetAbsoluteDmxAddress(int universe, int channel)
        {
            return ((universe-1) * 512) + channel;
        }
        
        private XmlElement GenerateFixtureElement(XmlDocument doc, JSONDMXFixtureData fixtureData)
        {
            var fixture = doc.CreateElement("Fixture");
            fixture.SetAttribute("uuid", Guid.NewGuid().ToString());
            fixture.SetAttribute("name", fixtureData.name);
            
            var matrix = doc.CreateElement("Matrix");
            matrix.InnerText = ToXMLTransformMatrix(fixtureData.position, fixtureData.rotation);
            
            var fixtureID = doc.CreateElement("FixtureID");
            fixtureID.InnerText = fixtureData.id.ToString();

            var addresses = doc.CreateElement("Addresses");
            var address = doc.CreateElement("Address");
            address.InnerText = GetAbsoluteDmxAddress(fixtureData.universe, fixtureData.channel).ToString();
            addresses.AppendChild(address);

            var dmxInvertPan = doc.CreateElement("DMXInvertPan");
            dmxInvertPan.InnerText = fixtureData.invertPan.ToString();
            var dmxInvertTilt = doc.CreateElement("DMXInvertTilt");
            dmxInvertTilt.InnerText = fixtureData.invertTilt.ToString();
            
            var unitNumber = doc.CreateElement("UnitNumber");
            unitNumber.InnerText = "0";

            var gdtfSpec = doc.CreateElement("GDTFSpec");
            gdtfSpec.InnerText = fixtureData.fixtureDefintion + ".gdtf";

            var gdtfMode = doc.CreateElement("GDTFMode");
            gdtfMode.InnerText = "Default";

            fixture.AppendChild(matrix);
            fixture.AppendChild(fixtureID);
            fixture.AppendChild(gdtfSpec);
            fixture.AppendChild(gdtfMode);
            fixture.AppendChild(unitNumber);
            fixture.AppendChild(addresses);
            fixture.AppendChild(dmxInvertPan);
            fixture.AppendChild(dmxInvertTilt);
            
            return fixture;
        }
        
        private string GeneralSceneDescriptionXML(JSONDMXFixtureData[] fixtureData)
        {
            var output = string.Empty;
            
            try
            {
                var xdoc = new XmlDocument();
                XmlNode docNode = xdoc.CreateXmlDeclaration("1.0", null, "no");
                xdoc.AppendChild(docNode);

                var version = VRSLStyles.GetVersion().Split(".");

                var generalSceneDescription = xdoc.CreateElement("GeneralSceneDescription");
                generalSceneDescription.SetAttribute("verMajor", version[0]);
                generalSceneDescription.SetAttribute("verMinor", version[1]);
                generalSceneDescription.SetAttribute("verPatch", version[2]);
                generalSceneDescription.SetAttribute("provider", "VR Stage Lighting");
                generalSceneDescription.SetAttribute("providerVersion", "1");
               
                var userData = xdoc.CreateElement("UserData");
                var scene = xdoc.CreateElement("Scene");
                var layers = xdoc.CreateElement("Layers");
                var layer = xdoc.CreateElement("Layer");

                layer.SetAttribute("name", "VRSL Main");
                layer.SetAttribute("uuid", Guid.NewGuid().ToString());
                var childList = xdoc.CreateElement("ChildList");
            
                foreach(var fixture in fixtureData)
                {
                    var f = GenerateFixtureElement(xdoc, fixture);
                    childList.AppendChild(f);
                }
                
                layer.AppendChild(childList);
                layers.AppendChild(layer);
                scene.AppendChild(layers);
                
                generalSceneDescription.AppendChild(userData);
                generalSceneDescription.AppendChild(scene);
               
                xdoc.AppendChild(generalSceneDescription);
                Debug.Log($"{Prefix}: Appending Scene Description");

                var sb = new StringBuilder();
                var sw = new StringWriterUtf8(sb);
                xdoc.Save(sw);
                output = sb.ToString();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }

            return output;
        }

        private bool CapContains(string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        private string FixtureDefinitionXML(string fixtureName, string[] channels)
        {
            string output = string.Empty;
            
            try
            {
                var xdoc = new XmlDocument();
                XmlNode docNode = xdoc.CreateXmlDeclaration("1.0", null, "no");
                xdoc.AppendChild(docNode);

                var gdtf = xdoc.CreateElement("GDTF");
                gdtf.SetAttribute("DataVersion", "1.1");
                var fixtureType = xdoc.CreateElement("FixtureType");
                fixtureType.SetAttribute("Name", fixtureName);
                fixtureType.SetAttribute("ShortName", fixtureName);
                fixtureType.SetAttribute("LongName", fixtureName);
                fixtureType.SetAttribute("Manufacturer", "VR Stage Lighting");
                fixtureType.SetAttribute("FixtureTypeID", Guid.NewGuid().ToString());
                fixtureType.SetAttribute("RefFT", "");
                
                var attributeDefinitions = xdoc.CreateElement("AttributeDefinitions");
                var featureGroups = xdoc.CreateElement("FeatureGroups");
                var hasMovement = false;
                var hasRGB = false;
                var hasDimmer = false;
                var hasStrobe = false;
                var hasZoom = false;
                var hasGobo = false;
                var hasLaserBeam = false;
                var hasFog = false;
                var comp = StringComparison.OrdinalIgnoreCase;
                
                foreach (var c in channels)
                {
                    if(!hasMovement && (CapContains(c,"pan",comp) || CapContains(c,"tilt",comp) || CapContains(c,"movement",comp) || CapContains(c,"rotate",comp)))
                    {
                        hasMovement = true;
                        continue;
                    }
                    if(!hasRGB && (CapContains(c,"red",comp) || CapContains(c,"green",comp) || CapContains(c,"blue",comp) || CapContains(c,"gradient",comp) || CapContains(c,"colour",comp) || CapContains(c,"color",comp)))
                    {
                        hasRGB = true;
                        continue;
                    }
                    if(!hasDimmer && (CapContains(c,"dimmer",comp) || CapContains(c,"intensity",comp) || CapContains(c,"power",comp)))
                    {
                        hasDimmer = true;
                        continue;
                    }
                    if(!hasStrobe && (CapContains(c,"strobe",comp) || CapContains(c,"shutter",comp) || CapContains(c,"flash",comp)))
                    {
                        hasStrobe = true;
                        continue;
                    }
                    if(!hasZoom && (CapContains(c,"zoom",comp) || CapContains(c,"focus",comp) || CapContains(c,"cone",comp)))
                    {
                        hasZoom = true;
                        continue;
                    }
                    if(!hasGobo && (CapContains(c,"gobo",comp) || CapContains(c,"cookie",comp)))
                    {
                        hasGobo = true;
                        continue;
                    }
                    if(!hasLaserBeam && (CapContains(c,"laser",comp) || CapContains(c,"beam",comp)))
                    {
                        hasLaserBeam = true;
                        continue;
                    }
                    if(!hasFog && (CapContains(c,"fog",comp) || CapContains(c,"c02",comp)))
                    {
                        hasFog = true;
                    }
                }

                if (hasMovement)
                {
                    var featureGroupMovement = xdoc.CreateElement("FeatureGroup");
                    featureGroupMovement.SetAttribute("Name", "Position");
                    featureGroupMovement.SetAttribute("Pretty", "Position");
                    
                    var featurePt = xdoc.CreateElement("Feature");    
                    featurePt.SetAttribute("Name", "PanTilt");

                    var featureRotate = xdoc.CreateElement("Feature");    
                    featureRotate.SetAttribute("Name", "Rotate");

                    featureGroupMovement.AppendChild(featurePt);
                    featureGroupMovement.AppendChild(featureRotate);

                    featureGroups.AppendChild(featureGroupMovement);
                }
                if(hasRGB)
                {
                    var featureGroupRGB = xdoc.CreateElement("FeatureGroup");
                    featureGroupRGB.SetAttribute("Name", "Color");
                    featureGroupRGB.SetAttribute("Pretty", "Color");        

                    var featureRGB = xdoc.CreateElement("Feature");    
                    featureRGB.SetAttribute("Name", "RGB");
                    var featureColor = xdoc.CreateElement("Feature");    
                    featureColor.SetAttribute("Name", "Color");
                    featureGroupRGB.AppendChild(featureRGB);
                    featureGroupRGB.AppendChild(featureColor);   

                    featureGroups.AppendChild(featureGroupRGB);      
                }
                if (hasDimmer)
                {
                    var featureGroupDimmer = xdoc.CreateElement("FeatureGroup");
                    featureGroupDimmer.SetAttribute("Name", "Dimmer");
                    featureGroupDimmer.SetAttribute("Pretty", "Dimmer");  

                    var featureDimmer = xdoc.CreateElement("Feature");    
                    featureDimmer.SetAttribute("Name", "Dimmer");
                    featureGroupDimmer.AppendChild(featureDimmer); 

                    featureGroups.AppendChild(featureGroupDimmer);                      
                }
                if (hasStrobe)
                {
                    var featureGroupStrobe = xdoc.CreateElement("FeatureGroup");
                    featureGroupStrobe.SetAttribute("Name", "Shutter");
                    featureGroupStrobe.SetAttribute("Pretty", "Shutter");     

                    var featureStrobe = xdoc.CreateElement("Feature");    
                    featureStrobe.SetAttribute("Name", "Shutter");
                    featureGroupStrobe.AppendChild(featureStrobe); 

                    featureGroups.AppendChild(featureGroupStrobe);                          
                }
                if (hasZoom)
                {
                    var featureGroupZoom = xdoc.CreateElement("FeatureGroup");
                    featureGroupZoom.SetAttribute("Name", "Focus");
                    featureGroupZoom.SetAttribute("Pretty", "Focus");

                    var featureZoom = xdoc.CreateElement("Feature");    
                    featureZoom.SetAttribute("Name", "Focus");
                    featureGroupZoom.AppendChild(featureZoom);  

                    featureGroups.AppendChild(featureGroupZoom);                            
                }
                if (hasGobo)
                {
                    var featureGroupGobo = xdoc.CreateElement("FeatureGroup");
                    featureGroupGobo.SetAttribute("Name", "Gobo");
                    featureGroupGobo.SetAttribute("Pretty", "Gobo");     

                    var featureGobo = xdoc.CreateElement("Feature");    
                    featureGobo.SetAttribute("Name", "Gobo");
                    featureGroupGobo.AppendChild(featureGobo);   

                    featureGroups.AppendChild(featureGroupGobo);                       
                }
                if (hasLaserBeam)
                {
                    var featureGroupLaser= xdoc.CreateElement("FeatureGroup");
                    featureGroupLaser.SetAttribute("Name", "Laser");
                    featureGroupLaser.SetAttribute("Pretty", "Laser"); 

                    var featureLaser = xdoc.CreateElement("Feature");    
                    featureLaser.SetAttribute("Name", "Laser");
                    featureGroupLaser.AppendChild(featureLaser);

                    featureGroups.AppendChild(featureGroupLaser);  
                }
                if (hasFog)
                {
                    var featureGroupFog = xdoc.CreateElement("FeatureGroup");
                    featureGroupFog.SetAttribute("Name", "Fog");
                    featureGroupFog.SetAttribute("Pretty", "Fog");     

                    var featureFog = xdoc.CreateElement("Feature");    
                    featureFog.SetAttribute("Name", "Fog");
                    featureGroupFog.AppendChild(featureFog);  

                    featureGroups.AppendChild(featureGroupFog);               
                }

                var featureGroupControl = xdoc.CreateElement("FeatureGroup");
                featureGroupControl.SetAttribute("Name", "Control");
                featureGroupControl.SetAttribute("Pretty", "Control");
                
                var featureControl = xdoc.CreateElement("Feature");    
                featureControl.SetAttribute("Name", "Control");
                featureGroupControl.AppendChild(featureControl);

                featureGroups.AppendChild(featureGroupControl);   

                var featureGroupOther = xdoc.CreateElement("FeatureGroup");
                featureGroupOther.SetAttribute("Name", "FeatureGroup");
                featureGroupOther.SetAttribute("Pretty", "FeatureGroup");
                
                var featureOther = xdoc.CreateElement("Feature");    
                featureOther.SetAttribute("Name", "Feature");
                featureGroupOther.AppendChild(featureOther);

                featureGroups.AppendChild(featureGroupOther);   

                var numOfGobos = 8;
                
                attributeDefinitions.AppendChild(featureGroups);

                #region Attributes
                var attributes = xdoc.CreateElement("Attributes");
                var hasFinePan = false;
                var panChan = 0;
                var finePanChan = 0;
                var hasFineTilt = false;
                var tiltChan = 0;
                var fineTiltChan = 0;
                var hasFineRotate = false;
                var rotateChan = 0;
                var fineRotateChan = 0;
                
                for(var i = 0; i < channels.Length; i++)
                {
                    var c = channels[i];
                    if (CapContains(c,"pan",comp) || CapContains(c,"tilt",comp) || CapContains(c,"rotate",comp))
                    {
                        if (CapContains(c,"pan",comp))
                        {
                            if (CapContains(c,"fine",comp))
                            {
                                hasFinePan = true; 
                                finePanChan = i; 
                                continue;
                            }
                            else
                            {
                                panChan = i;
                            }
                        }
                        if(CapContains(c,"tilt",comp))
                        {
                            if(CapContains(c,"fine",comp))
                            {
                                hasFineTilt = true; 
                                fineTiltChan = i; 
                                continue;
                            }
                            else
                            {
                                tiltChan = i;
                            }
                        }
                        if(CapContains(c,"rotate",comp))
                        {
                            if(CapContains(c,"fine",comp))
                            {
                                hasFineRotate = true; 
                                fineRotateChan = i; 
                                continue;
                            }
                            else
                            {
                                rotateChan = i; 
                            }
                        }
                        var attribute = xdoc.CreateElement("Attribute");
                        attribute.SetAttribute("Name", c);
                        attribute.SetAttribute("Pretty", c);
                        attribute.SetAttribute("PhysicalUnit", "Angle");
                        if(CapContains(c,"pan",comp) || CapContains(c,"tilt",comp))
                        {
                            attribute.SetAttribute("Feature", "Position.PanTilt");
                        }
                        else
                        {
                            attribute.SetAttribute("Feature", "Position.Rotate");
                        }
                        attributes.AppendChild(attribute);
                        continue;
                    }
                    else if(CapContains(c,"red",comp) || CapContains(c,"green",comp) || CapContains(c,"blue",comp) || CapContains(c,"gradient",comp) || CapContains(c,"colour",comp) || CapContains(c,"color",comp))
                    {
                        var attribute = xdoc.CreateElement("Attribute");
                        attribute.SetAttribute("Name", c);
                        attribute.SetAttribute("Pretty", c);
                        attribute.SetAttribute("PhysicalUnit", "None");
                        if(CapContains(c,"red",comp) || CapContains(c,"green",comp) || CapContains(c,"blue",comp))
                        {
                            attribute.SetAttribute("Feature", "Color.RGB");
                        }
                        else
                        {
                            attribute.SetAttribute("Feature", "Color.Color");
                        }
                        attributes.AppendChild(attribute);
                        continue;
                    }
                    else if(CapContains(c,"dimmer",comp) || CapContains(c,"intensity",comp) || CapContains(c,"power",comp))
                    {
                        var attribute = xdoc.CreateElement("Attribute");
                        attribute.SetAttribute("Name", c);
                        attribute.SetAttribute("Pretty", c);
                        attribute.SetAttribute("PhysicalUnit", "None");
                        attribute.SetAttribute("Feature", "Dimmer.Dimmer");
                        attributes.AppendChild(attribute);
                        continue;
                    }
                    else if(CapContains(c,"strobe",comp) || CapContains(c,"shutter",comp) || CapContains(c,"flash",comp))
                    {
                        var attribute = xdoc.CreateElement("Attribute");
                        attribute.SetAttribute("Name", c);
                        attribute.SetAttribute("Pretty", c);
                        attribute.SetAttribute("PhysicalUnit", "None");
                        attribute.SetAttribute("Feature", "Shutter.Shutter");
                        attributes.AppendChild(attribute);
                        continue;
                    }
                    else if(CapContains(c,"zoom",comp) || CapContains(c,"focus",comp) || CapContains(c,"cone",comp))
                    {
                        var attribute = xdoc.CreateElement("Attribute");
                        attribute.SetAttribute("Name", c);
                        attribute.SetAttribute("Pretty", c);
                        attribute.SetAttribute("PhysicalUnit", "None");
                        attribute.SetAttribute("Feature", "Focus.Focus");
                        attributes.AppendChild(attribute);
                        continue;
                    }
                    else if(CapContains(c,"gobo",comp) || CapContains(c,"cookie",comp))
                    {
                        var attribute = xdoc.CreateElement("Attribute");
                        attribute.SetAttribute("Name", c);
                        attribute.SetAttribute("Pretty", c);
                        attribute.SetAttribute("PhysicalUnit", "None");
                        attribute.SetAttribute("Feature", "Gobo.Gobo");
                        attributes.AppendChild(attribute);
                        continue;
                    }
                    else if(CapContains(c,"laser",comp) || CapContains(c,"beam",comp))
                    {
                        var attribute = xdoc.CreateElement("Attribute");
                        attribute.SetAttribute("Name", c);
                        attribute.SetAttribute("Pretty", c);
                        attribute.SetAttribute("PhysicalUnit", "None");
                        attribute.SetAttribute("Feature", "Laser.Laser");
                        attributes.AppendChild(attribute);
                        continue;
                    }
                    else if(CapContains(c,"fog",comp) || CapContains(c,"c02",comp))
                    {
                        var attribute = xdoc.CreateElement("Attribute");
                        attribute.SetAttribute("Name", c);
                        attribute.SetAttribute("Pretty", c);
                        attribute.SetAttribute("PhysicalUnit", "None");
                        attribute.SetAttribute("Feature", "Fog.Fog");
                        attributes.AppendChild(attribute);
                        continue;
                    }
                    else
                    {
                        var attribute = xdoc.CreateElement("Attribute");
                        attribute.SetAttribute("Name", c);
                        attribute.SetAttribute("Pretty", c);
                        attribute.SetAttribute("PhysicalUnit", "None");
                        attribute.SetAttribute("Feature", "Control.Control");
                        attributes.AppendChild(attribute);
                        continue;                        
                    }
                }
                attributeDefinitions.AppendChild(attributes);
                #endregion

                var dmxModes = xdoc.CreateElement("DMXModes");

                var models = xdoc.CreateElement("Models");
                var model = xdoc.CreateElement("Model");
                model.SetAttribute("Name", "Base");
                models.AppendChild(model);

                var geometries = xdoc.CreateElement("Geometries");
                var geometry = xdoc.CreateElement("Geometry");
                geometry.SetAttribute("Model", "Base");
                geometry.SetAttribute("Name", "Base");
                geometry.SetAttribute("Position", "{1,0,0,0}{0,1,0,0}{0,0,1,0}{0,0,0,1}");

                geometries.AppendChild(geometry);

                fixtureType.AppendChild(attributeDefinitions);

                if (hasGobo)
                {
                    var wheels = xdoc.CreateElement("Wheels");
                    var goboWheel = xdoc.CreateElement("Wheel");
                    goboWheel.SetAttribute("Name", "GoboWheel");
                    var slotName = "Open";
                    
                    for(var i = 0; i < numOfGobos; i++)
                    {
                        var slot = xdoc.CreateElement("Slot");
                        if(i > 0)
                        {
                            slotName = "Gobo " + i;
                        }
                        slot.SetAttribute("Name", slotName);
                        goboWheel.AppendChild(slot);  
                    }
                    wheels.AppendChild(goboWheel);  
                    fixtureType.AppendChild(wheels);
                }

                fixtureType.AppendChild(models);
                fixtureType.AppendChild(geometries);
                
                var dmxMode = xdoc.CreateElement("DMXMode");
                dmxMode.SetAttribute("Description", "Default Channel Mode");
                dmxMode.SetAttribute("Geometry", "Base");
                dmxMode.SetAttribute("Name", "Default");

                var dmxChannels = xdoc.CreateElement("DMXChannels");
                
                for(var i = 0; i < channels.Length; i++)
                {
                    var highlight = "None";
                    var channelName = channels[i];
                    var channel = i + 1;
                    var fineCheck = false;
                    var dmxChannel = xdoc.CreateElement("DMXChannel");
                    if (CapContains(channelName,"pan",comp))
                    {
                        if (CapContains(channelName, "fine", comp))
                        {
                            continue;
                        }
                        dmxChannel.SetAttribute("DMXBreak", "1");
                        dmxChannel.SetAttribute("Geometry", "Base");
                        if (hasFinePan)
                        {
                            var offset = Mathf.Abs(finePanChan - panChan);
                            offset *= finePanChan < panChan ? -1 : 1;
                            dmxChannel.SetAttribute("Offset", channel + "," + (channel + offset));
                            fineCheck = true;
                        }
                        else
                        {
                            dmxChannel.SetAttribute("Offset", channel.ToString());
                        }
                    }
                    else if (CapContains(channelName,"tilt",comp))
                    {
                        if (CapContains(channelName, "fine", comp))
                        {
                            continue;
                        }
                        dmxChannel.SetAttribute("DMXBreak", "1");
                        dmxChannel.SetAttribute("Geometry", "Base");
                        if (hasFineTilt)
                        {
                            var offset = Mathf.Abs(fineTiltChan - tiltChan);
                            offset *= fineTiltChan < tiltChan ? -1 : 1;
                            dmxChannel.SetAttribute("Offset", channel + "," + (channel + offset));
                            fineCheck = true;
                        }
                        else
                        {
                            dmxChannel.SetAttribute("Offset", (channel).ToString());
                        }
                    }
                    else if (CapContains(channelName,"rotate",comp))
                    {
                        if (CapContains(channelName, "fine", comp))
                        {
                            continue;
                        }
                        dmxChannel.SetAttribute("DMXBreak", "1");
                        dmxChannel.SetAttribute("Geometry", "Base");
                        if (hasFineRotate)
                        {
                            var offset = Mathf.Abs(fineRotateChan - rotateChan);
                            offset *= fineRotateChan < rotateChan ? -1 : 1;
                            dmxChannel.SetAttribute("Offset", channel + "," + (channel + offset));
                            fineCheck = true;
                        }
                        else
                        {
                            dmxChannel.SetAttribute("Offset", channel.ToString());
                        }
                    }
                    else
                    {
                        dmxChannel.SetAttribute("DMXBreak", "1");
                        dmxChannel.SetAttribute("Geometry", "Base");
                        dmxChannel.SetAttribute("Offset", channel.ToString());
                    }
                    if (CapContains(channelName,"red",comp) || CapContains(channelName,"green",comp) || CapContains(channelName,"blue",comp) || CapContains(channelName,"dim",comp) || CapContains(channelName,"dimmer",comp) || CapContains(channelName,"intensity",comp))
                    {
                        highlight = "255/1";
                    }
                    
                    var master = "None";
                    if (CapContains(channelName,"dim",comp) || CapContains(channelName,"dimmer",comp))
                    {
                        master = "Grand";
                    }
                    
                    var dmxFrom =  fineCheck ? "0/2" : "0/1";
                    dmxChannel.SetAttribute("Highlight", highlight);
                    
                    var logicalChannel = xdoc.CreateElement("LogicalChannel");
                    logicalChannel.SetAttribute("Attribute", channelName);
                    logicalChannel.SetAttribute("Master", master);

                    var channelFunction = xdoc.CreateElement("ChannelFunction");
                    channelFunction.SetAttribute("Attribute", channelName);
                    channelFunction.SetAttribute("DMXFrom", dmxFrom);
                    channelFunction.SetAttribute("Name", channelName);
                    if ((CapContains(channelName,"gobo",comp) || CapContains(channelName,"cookie",comp)) && CapContains(channelName,"select",comp))
                    {   
                       channelFunction.SetAttribute("Wheel", "GoboWheel");
                       var goboName = "Open";
                       var fromVal = 0;
                       for(var j = 0; j < numOfGobos; j++)
                       {
                            var channelSet = xdoc.CreateElement("ChannelSet"); 
                            if (j > 0)
                            {
                                goboName = "Gobo " + j;
                                fromVal += Mathf.FloorToInt(255 / numOfGobos);
                            }
                            channelSet.SetAttribute("Name", goboName);
                            channelSet.SetAttribute("DMXFrom", fromVal + "/1");
                            channelSet.SetAttribute("PhysicalFrom", "0");
                            channelSet.SetAttribute("PhysicalTo", "0");
                            channelSet.SetAttribute("WheelSlotIndex", (j+1).ToString());
                            channelFunction.AppendChild(channelSet);
                       }
                    }

                    logicalChannel.AppendChild(channelFunction);
                    dmxChannel.AppendChild(logicalChannel);
                    dmxChannels.AppendChild(dmxChannel);
                }

                dmxMode.AppendChild(dmxChannels);
                dmxModes.AppendChild(dmxMode);
                fixtureType.AppendChild(dmxModes);
                gdtf.AppendChild(fixtureType);
                xdoc.AppendChild(gdtf);

                var sb = new StringBuilder();
                var sw = new StringWriterUtf8(sb);
                xdoc.Save(sw);
                output = sb.ToString();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
            return output;
        }
        
        public void ToMVRFile()
        {
            if (!CheckForLocalPanel()) return;
            
            var fixDefAsset = (VRSL_FixtureDefinitions) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_panel.fixtureDefGUID), typeof(VRSL_FixtureDefinitions));
            var jsonFixtureData = new JSONDMXFixtureData[data.Length];
            var definitions = fixDefAsset.GetNames();
            for(var i = 0; i < jsonFixtureData.Length; i++)
            {
                jsonFixtureData[i] = new JSONDMXFixtureData(data[i], definitions, fixDefAsset.GetChannelDefinition(data[i].fixtureDefintion));
            }
                
            var assetPath = AssetDatabase.GetAssetPath(this);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            var targetString = name + ".asset";
            var newString = name + ".mvr";
            filePath = filePath.Replace("/", "\\");
            filePath = filePath.Replace(targetString, newString);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.LogWarning($"{Prefix}: File '{filePath}' already exists, overwriting.");
            }
            using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    //CREATE SCENE DESCRIPTION ////////////////////////////////////////
                    var sceneDescriptionBytes = Encoding.ASCII.GetBytes(GeneralSceneDescriptionXML(jsonFixtureData));
                    var sceneDescriptionName = "GeneralSceneDescription.xml";
                    var sceneDescriptionEntry = archive.CreateEntry(sceneDescriptionName, System.IO.Compression.CompressionLevel.NoCompression);
                    using (var zipStream = sceneDescriptionEntry.Open())
                    {
                        zipStream.Write(sceneDescriptionBytes, 0, sceneDescriptionBytes.Length);
                    }
                    /////////////////////////////////////////////////////////////////
                    for(var i = 0; i < fixDefAsset.definitions.Length; i++)
                    {
                        var defName = fixDefAsset.definitions[i].name + ".gdtf";
                        var fixDefEntry = archive.CreateEntry(defName, System.IO.Compression.CompressionLevel.NoCompression);
                            
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var defArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                            {
                                var descriptionBytes = Encoding.ASCII.GetBytes(FixtureDefinitionXML(fixDefAsset.definitions[i].name, fixDefAsset.definitions[i].channelNames));
                                var descriptionEntry = defArchive.CreateEntry("description.xml", System.IO.Compression.CompressionLevel.NoCompression);
                                using (var fixDefZipStream = descriptionEntry.Open())
                                {
                                    fixDefZipStream.Write(descriptionBytes, 0, descriptionBytes.Length);
                                } 
                                using (var zipStream = fixDefEntry.Open())
                                {
                                    defArchive.Dispose();
                                    zipStream.Write(memoryStream.ToArray(), 0, memoryStream.ToArray().Length);
                                }
                            }

                            memoryStream.Dispose();
                        }
                    }
                }
            }
            AssetDatabase.Refresh();
            Debug.Log($"{Prefix}: Successfully Exported MVR File");
        }
    }
}
#endif