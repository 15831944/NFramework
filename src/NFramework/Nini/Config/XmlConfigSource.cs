#region Copyright

//
// Nini Configuration Project.
// Copyright (C) 2006 Brent R. Matzelle.  All rights reserved.
//
// This software is published under the terms of the MIT X11 license, a copy of 
// which has been included with this distribution in the LICENSE.txt file.
// 

#endregion

using System;
using System.IO;
using System.Xml;

namespace NSoft.NFramework.Nini.Config {
    public class XmlConfigSource : ConfigSourceBase {
        private XmlDocument configDoc = null;
        private string savePath = null;

        #region Constructors

        public XmlConfigSource() {
            configDoc = new XmlDocument();
            configDoc.LoadXml("<Nini/>");
            PerformLoad(configDoc);
        }

        public XmlConfigSource(string path) {
            Load(path);
        }

        public XmlConfigSource(XmlReader reader) {
            Load(reader);
        }

        #endregion

        public string SavePath {
            get { return savePath; }
        }

        public void Load(string path) {
            savePath = path;
            configDoc = new XmlDocument();
            configDoc.Load(path);
            PerformLoad(configDoc);
        }

        public void Load(XmlReader reader) {
            configDoc = new XmlDocument();
            configDoc.Load(reader);
            PerformLoad(configDoc);
        }

        public override void Save() {
            if(!IsSavable()) {
                throw new InvalidOperationException("Source cannot be saved in this state");
            }

            MergeConfigsIntoDocument();
            configDoc.Save(savePath);
            base.Save();
        }

        public void Save(string path) {
            savePath = path;
            Save();
        }

        public void Save(TextWriter writer) {
            MergeConfigsIntoDocument();
            configDoc.Save(writer);
            savePath = null;
            OnSaved(new EventArgs());
        }

        public void Save(Stream stream) {
            MergeConfigsIntoDocument();
            configDoc.Save(stream);
            savePath = null;
            OnSaved(new EventArgs());
        }

        public override void Reload() {
            if(savePath == null) {
                throw new ArgumentException("Error reloading: You must have "
                                            + "the loaded the source from a file");
            }

            configDoc = new XmlDocument();
            configDoc.Load(savePath);
            MergeDocumentIntoConfigs();
            base.Reload();
        }

        public override string ToString() {
            MergeConfigsIntoDocument();

            using(var writer = new StringWriter()) {
                configDoc.Save(writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Merges all of the configs from the config collection into the 
        /// XmlDocument.
        /// </summary>
        private void MergeConfigsIntoDocument() {
            RemoveSections();
            foreach(IConfig config in Configs) {
                string[] keys = config.GetKeys();

                XmlNode node = GetSectionByName(config.Name);
                if(node == null) {
                    node = SectionNode(config.Name);
                    configDoc.DocumentElement.AppendChild(node);
                }
                RemoveKeys(config.Name);

                for(int i = 0; i < keys.Length; i++) {
                    SetKey(node, keys[i], config.Get(keys[i]));
                }
            }
        }

        /// <summary>
        /// Removes all XML sections that were removed as configs.
        /// </summary>
        private void RemoveSections() {
            XmlAttribute attr = null;

            foreach(XmlNode node in configDoc.DocumentElement.ChildNodes) {
                if(node.NodeType == XmlNodeType.Element
                   && node.Name == "Section") {
                    attr = node.Attributes["Name"];
                    if(attr != null) {
                        if(Configs[attr.Value] == null) {
                            configDoc.DocumentElement.RemoveChild(node);
                        }
                    }
                    else {
                        throw new ArgumentException("Section name attribute not found");
                    }
                }
            }
        }

        /// <summary>
        /// Removes all XML keys that were removed as config keys.
        /// </summary>
        private void RemoveKeys(string sectionName) {
            XmlNode sectionNode = GetSectionByName(sectionName);
            XmlAttribute keyName = null;

            if(sectionNode != null) {
                foreach(XmlNode node in sectionNode.ChildNodes) {
                    if(node.NodeType == XmlNodeType.Element
                       && node.Name == "Key") {
                        keyName = node.Attributes["Name"];
                        if(keyName != null) {
                            if(Configs[sectionName].Get(keyName.Value) == null) {
                                sectionNode.RemoveChild(node);
                            }
                        }
                        else {
                            throw new ArgumentException("Name attribute not found in key");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads all sections and keys.
        /// </summary>
        private void PerformLoad(XmlDocument document) {
            Configs.Clear();

            Merge(this); // required for SaveAll

            if(document.DocumentElement.Name != "Nini") {
                throw new ArgumentException("Did not find Nini XML root node");
            }

            LoadSections(document.DocumentElement);
        }

        /// <summary>
        /// Loads all configuration sections.
        /// </summary>
        private void LoadSections(XmlNode rootNode) {
            ConfigBase config = null;

            foreach(XmlNode child in rootNode.ChildNodes) {
                if(child.NodeType == XmlNodeType.Element
                   && child.Name == "Section") {
                    config = new ConfigBase(child.Attributes["Name"].Value, this);
                    Configs.Add(config);
                    LoadKeys(child, config);
                }
            }
        }

        /// <summary>
        /// Loads all keys for a config.
        /// </summary>
        private void LoadKeys(XmlNode node, ConfigBase config) {
            foreach(XmlNode child in node.ChildNodes) {
                if(child.NodeType == XmlNodeType.Element
                   && child.Name == "Key") {
                    config.Add(child.Attributes["Name"].Value,
                               child.Attributes["Value"].Value);
                }
            }
        }

        /// <summary>
        /// Sets an XML key.  If it does not exist then it is created.
        /// </summary>
        private void SetKey(XmlNode sectionNode, string key, string value) {
            XmlNode node = GetKeyByName(sectionNode, key);

            if(node == null) {
                CreateKey(sectionNode, key, value);
            }
            else {
                node.Attributes["Value"].Value = value;
            }
        }

        /// <summary>
        /// Creates a key node and adds it to the collection at the end.
        /// </summary>
        private void CreateKey(XmlNode sectionNode, string key, string value) {
            XmlNode node = configDoc.CreateElement("Key");
            XmlAttribute keyAttr = configDoc.CreateAttribute("Name");
            XmlAttribute valueAttr = configDoc.CreateAttribute("Value");
            keyAttr.Value = key;
            valueAttr.Value = value;

            node.Attributes.Append(keyAttr);
            node.Attributes.Append(valueAttr);

            sectionNode.AppendChild(node);
        }

        /// <summary>
        /// Returns a new section node.
        /// </summary>
        private XmlNode SectionNode(string name) {
            XmlNode result = configDoc.CreateElement("Section");
            XmlAttribute nameAttr = configDoc.CreateAttribute("Name");
            nameAttr.Value = name;
            result.Attributes.Append(nameAttr);

            return result;
        }

        /// <summary>
        /// Returns a section node by name.
        /// </summary>
        private XmlNode GetSectionByName(string name) {
            XmlNode result = null;

            foreach(XmlNode node in configDoc.DocumentElement.ChildNodes) {
                if(node.NodeType == XmlNodeType.Element
                   && node.Name == "Section"
                   && node.Attributes["Name"].Value == name) {
                    result = node;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a key node by name.
        /// </summary>
        private XmlNode GetKeyByName(XmlNode sectionNode, string name) {
            XmlNode result = null;

            foreach(XmlNode node in sectionNode.ChildNodes) {
                if(node.NodeType == XmlNodeType.Element
                   && node.Name == "Key"
                   && node.Attributes["Name"].Value == name) {
                    result = node;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if this instance is savable.
        /// </summary>
        private bool IsSavable() {
            return (savePath != null
                    && configDoc != null);
        }

        /// <summary>
        /// Merges the XmlDocument into the Configs when the document is 
        /// reloaded.  
        /// </summary>
        private void MergeDocumentIntoConfigs() {
            // Remove all missing configs first
            RemoveConfigs();

            foreach(XmlNode node in configDoc.DocumentElement.ChildNodes) {
                // If node is a section node
                if(node.NodeType == XmlNodeType.Element
                   && node.Name == "Section") {
                    string sectionName = node.Attributes["Name"].Value;
                    IConfig config = Configs[sectionName];
                    if(config == null) {
                        // The section is new so add it
                        config = new ConfigBase(sectionName, this);
                        Configs.Add(config);
                    }
                    RemoveConfigKeys(config);
                }
            }
        }

        /// <summary>
        /// Removes all configs that are not in the newly loaded XmlDocument.  
        /// </summary>
        private void RemoveConfigs() {
            IConfig config = null;
            for(int i = Configs.Count - 1; i > -1; i--) {
                config = Configs[i];
                // If the section is not present in the XmlDocument
                if(GetSectionByName(config.Name) == null) {
                    Configs.Remove(config);
                }
            }
        }

        /// <summary>
        /// Removes all XML keys that were removed as config keys.
        /// </summary>
        private void RemoveConfigKeys(IConfig config) {
            XmlNode section = GetSectionByName(config.Name);

            // Remove old keys
            string[] configKeys = config.GetKeys();
            foreach(string configKey in configKeys) {
                if(GetKeyByName(section, configKey) == null) {
                    // Key doesn't exist, remove
                    config.Remove(configKey);
                }
            }

            // Add or set all new keys
            foreach(XmlNode node in section.ChildNodes) {
                // Loop through all key nodes and add to config
                if(node.NodeType == XmlNodeType.Element && node.Name == "Key") {
                    config.Set(node.Attributes["Name"].Value,
                               node.Attributes["Value"].Value);
                }
            }
        }
    }
}