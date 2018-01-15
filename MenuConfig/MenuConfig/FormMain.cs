using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Xml;
using WinUtils;
using WinUtils.Dialogs;

namespace MenuConfig
{
    public partial class FormMain : Form
    {
        class ItemProperty
        {
            public string Command, Arguments;
            public bool IsSubMenu;
            public string IconFile;
            public int IconIndex;
            public int IconSize;

            public bool HasIcon()
            {
                return IconFile != null;
            }

            public bool HasCustomIconSize()
            {
                return IconSize != 0;
            }
        }
        string currentFile;


        #region UI
        const int WS_CLIPCHILDREN = 0x2000000;
        const int WS_THICKFRAME = 0x00040000;
        const int WS_MINIMIZEBOX = 0x20000;
        const int WS_MAXIMIZEBOX = 0x10000;
        const int WS_SYSMENU = 0x80000;
        const int WS_CHILD = 0x40000000;
        const int CS_DBLCLKS = 0x8;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                cp.Style |= WS_CLIPCHILDREN | WS_MINIMIZEBOX | WS_SYSMENU;
                cp.Style &= ~WS_MAXIMIZEBOX;
                cp.Style &= ~WS_CHILD;
                cp.ClassStyle = CS_DBLCLKS;

                return cp;
            }
        }

        void ModifySysMenu()
        {
            IntPtr sysMenu = WinAPI.GetSystemMenu(this.Handle, false);
            WinAPI.EnableMenuItem(sysMenu, Constants.SC_CLOSE, 0x00000000);
            WinAPI.EnableMenuItem(sysMenu, Constants.SC_SIZE, 0x00000001);
        }

        private void panelExtended_Paint(object sender, PaintEventArgs e)
        {
            if (WinAPI.GetForegroundWindow() == this.Handle) 
                Win7Style.DrawTextOnGlass(panelExtended.Handle, labelCaption.Text, labelCaption.Font, labelCaption.Bounds, 12);
            else
                Win7Style.DrawTextOnGlass(panelExtended.Handle, labelCaption.Text, labelCaption.Font, labelCaption.Bounds, 12);
        }

        private void FormMain_Deactivate(object sender, EventArgs e)
        {
            labelCaption.ForeColor = SystemColors.InactiveCaption;
            panelFrameB.BackColor = SystemColors.InactiveBorder;
            panelFrameT.BackColor = SystemColors.InactiveBorder;
            panelFrameR.BackColor = SystemColors.InactiveBorder;
            panelFrameL.BackColor = SystemColors.InactiveBorder;
            pictureBoxClose.BackgroundImage = MenuConfig.Properties.Resources.CloseI;
            pictureBoxMinimize.BackgroundImage = MenuConfig.Properties.Resources.MinimizeI;

            ModifySysMenu();panelExtended.Refresh();
        }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            labelCaption.ForeColor = SystemColors.WindowText;
            panelFrameB.BackColor = SystemColors.ActiveBorder;
            panelFrameT.BackColor = SystemColors.ActiveBorder;
            panelFrameR.BackColor = SystemColors.ActiveBorder;
            panelFrameL.BackColor = SystemColors.ActiveBorder;
            pictureBoxClose.BackgroundImage = MenuConfig.Properties.Resources.Close;
            pictureBoxMinimize.BackgroundImage = MenuConfig.Properties.Resources.Minimize;

            ModifySysMenu(); panelExtended.Refresh();
        }

        private void panelExtended_MouseDown(object sender, MouseEventArgs e)
        {
            FormUtils.DragWindow(this.Handle);
        }

        private void panelExtended_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                FormUtils.ShowSystemMenu(this.Handle);
        }

        private void pictureBoxMinimize_MouseEnter(object sender, EventArgs e)
        {
            pictureBoxMinimize.BackgroundImage = MenuConfig.Properties.Resources.MinimizeS;
        }

        private void pictureBoxMinimize_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxMinimize.BackgroundImage = MenuConfig.Properties.Resources.Minimize;
        }

        private void pictureBoxMinimize_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBoxMinimize.BackgroundImage = MenuConfig.Properties.Resources.MinimizeP;
        }

        private void pictureBoxMinimize_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBoxMinimize.BackgroundImage = MenuConfig.Properties.Resources.Minimize;
        }

        private void pictureBoxClose_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBoxClose.BackgroundImage = MenuConfig.Properties.Resources.CloseP;
        }

        private void pictureBoxClose_MouseEnter(object sender, EventArgs e)
        {
            pictureBoxClose.BackgroundImage = MenuConfig.Properties.Resources.CloseS;
        }

        private void pictureBoxClose_MouseLeave(object sender, EventArgs e)
        {
            pictureBoxClose.BackgroundImage = MenuConfig.Properties.Resources.Close;
        }

        private void pictureBoxClose_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBoxClose.BackgroundImage = MenuConfig.Properties.Resources.Close;
        }

        private void pictureBoxAppIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBoxMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0084)
            {
                base.WndProc(ref m);



                if ((IntPtr)12 == m.Result || (IntPtr)15 == m.Result || (IntPtr)10 == m.Result || (IntPtr)11 == m.Result
                 || (IntPtr)16 == m.Result || (IntPtr)17 == m.Result || (IntPtr)13 == m.Result || (IntPtr)14 == m.Result)
                    m.Result = (IntPtr)1;

            }
            else if (m.Msg == 0x0112)
            {
                ModifySysMenu();

                if (m.WParam != (IntPtr)0xF000)
                    base.WndProc(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        #endregion

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Icon = IconManager.GetIcon("shell32.dll", 39, true);

            if (!System.IO.File.Exists(Installer.GetMenu98Path()))
                Installer.ShowInstaller(this);


            if (WinVer.IsWin10OrGreater())
            {
                Win10Style.EnableBlur(this.Handle);
            }
            else
            {
                checkBoxNoExplorerImmersiveMenu.Enabled = false;
                checkBoxEnableImmersiveMenu.Enabled = false;
                checkBoxShowIcon.Enabled = false;
                checkBoxHideToggleOption.Enabled = false;
            }
                
            if (WinVer.IsVistaOrGreater())
            {
                //DPI.SetCurrentProcessDPIAware();
                Win7Style.EnableAero(this.Handle, panelExtended.Height, panelMain.Bottom, 0, 0);
                toolStrip.Renderer = new ToolStripNonClientRender();
            }
            else
            {
                toolStrip.Renderer = new ToolStripNonClientRender(SystemColors.Control);
                toolStrip.Location = new Point(toolStrip.Location.X, toolStrip.Location.Y - 1);
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.ControlBox = true;
                this.Height -= panelMain.Location.Y;
                this.panelMain.Location = new Point(panelMain.Location.X, 0);
                this.Text = labelCaption.Text;
            }

            panelFrameB.Visible = false;
            panelFrameT.Visible = false;
            panelFrameR.Visible = false;
            panelFrameL.Visible = false;

            pictureBoxAppIcon.BackgroundImage = this.Icon.ToBitmap();


            contextMenuStrip.Renderer = new ToolStripNonClientRender();

            comboBoxStyle.SelectedIndex = 0;

            FormUtils.ShowShield(buttonUpdateStartupCommand.Handle, true);
            FormUtils.ShowShield(buttonRemoveStartUp.Handle, true);
            
            for (int i=0; i<= MenuControl.OSList.Length; i++)
                comboBoxSysVer.Items.Add(MenuControl.GetOSFromIndex(i));
            comboBoxSysVer.SelectedIndex = MenuControl.Os2ListIndex(WinVer.SystemVersion);

            checkBoxNoExplorerImmersiveMenu.Checked = Installer.ExplorerImmersiveMenuRemoved(); 


            if (System.IO.File.Exists(Installer.GetConfigPath()))
            {
                XML_Load(Installer.GetConfigPath());
            }
            else
            {
                switch(Installer.ShowSelectConfigFileBox(this))
                {
                    case DialogResult.Yes:
                        Installer.DeployDefaultConfigFile();
                        XML_Load(Installer.GetConfigPath());
                        break;
                    case DialogResult.No:
                        if (!OpenConfigUsingDialog())
                            this.Close();
                        break;
                    case DialogResult.Cancel:
                        this.Close();
                        break;
                }
            }
        }

        #region imageList
        string GetAbsolutePath(string path)
        {
            if (System.IO.Path.GetPathRoot(path) == "")
            {         
                //TO-DO:   
                foreach (string sysPath in Environment.GetEnvironmentVariable("Path").Split(';'))
                {
                    string ret = Environment.ExpandEnvironmentVariables(sysPath);
                    ret = System.IO.Path.Combine(ret, path);
                    if (System.IO.File.Exists(ret))
                        return ret;
                }
                return null;
            }
            else
            {
                return path;
            }
        }

        void imageList_Init()
        {
            imageList.Images.Clear();
            imageList.Images.Add("Item", MenuConfig.Properties.Resources.Item);
            imageList.Images.Add("SubMenu", MenuConfig.Properties.Resources.SubMenu);
            imageList.Images.Add("Seperator", MenuConfig.Properties.Resources.Seperator);
        }

        string imageList_GetKey(string iconFile, int iconIndex)
        {
            return GetAbsolutePath(iconFile) + ":" + iconIndex.ToString();
        }

        string imageList_AddIcon(string iconFile, int iconIndex)
        {
            Icon icon = IconManager.GetIcon(iconFile, iconIndex, false);
            if (icon == null)
                return "";
            else
            {
                string key = imageList_GetKey(iconFile, iconIndex);
                if (!imageList.Images.ContainsKey(key))
                    imageList.Images.Add(key, icon);
                icon.Dispose();
                
                return key;
            }

        }
        #endregion

        #region xml
        public bool OpenConfigUsingDialog()
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                Filter = "Xml documents(*.xml)|*.xml|All files(*.*)|*.*",
                CheckFileExists = true
            };

            if (dlg.ShowDialog(this) == DialogResult.Cancel)
                return false;

            XML_Load(dlg.FileName);
            return true;
        }

        string XML_GetNodeAttribute(XmlNode xmlNode, string name, string def = null)
        {
            XmlNode attrib = xmlNode.Attributes.GetNamedItem(name);
            return attrib == null ? def : attrib.Value;
        }

        int XML_GetNodeAttributeInt(XmlNode xmlNode, string name, int def = 0)
        {
            XmlNode attrib = xmlNode.Attributes.GetNamedItem(name);
            return attrib == null ? def : int.Parse(attrib.Value);
        }

        void XML_Config_Load(XmlNode rootNode)
        {
            comboBoxStyle.SelectedIndex = XML_GetNodeAttributeInt(rootNode, "style");
            if (XML_GetNodeAttribute(rootNode, "popup") == "0")
                radioButtonLegacyStart.Checked = true;
            else
                radioButtonPopup.Checked = true;
            textBoxCaption.Text = XML_GetNodeAttribute(rootNode, "text", "");
            numericUpDownCaptionSize.Value = XML_GetNodeAttributeInt(rootNode, "captionWidth");
            numericUpDownCaptionWidth.Value = XML_GetNodeAttributeInt(rootNode, "titleBarWidth");
            numericUpDownLargeIconSize.Value = XML_GetNodeAttributeInt(rootNode, "largeIconSize");
            numericUpDownSmallIconSize.Value = XML_GetNodeAttributeInt(rootNode, "smallIconSize");
            numericUpDownBlankWidth.Value = XML_GetNodeAttributeInt(rootNode, "blankWidth");
            numericUpDownBlankHeight.Value = XML_GetNodeAttributeInt(rootNode, "blankHeight");
        }

        void XML_Config_Save(XmlElement rootNode)
        {
            rootNode.SetAttribute("style", comboBoxStyle.SelectedIndex.ToString());
            rootNode.SetAttribute("popup", radioButtonPopup.Checked ? "1" : "0");
            rootNode.SetAttribute("text", textBoxCaption.Text);
            rootNode.SetAttribute("captionWidth", numericUpDownCaptionSize.Value.ToString());
            rootNode.SetAttribute("titleBarWidth", numericUpDownCaptionWidth.Value.ToString());
            rootNode.SetAttribute("largeIconSize", numericUpDownLargeIconSize.Value.ToString());
            rootNode.SetAttribute("smallIconSize", numericUpDownSmallIconSize.Value.ToString());
            rootNode.SetAttribute("blankWidth", numericUpDownBlankWidth.Value.ToString());
            rootNode.SetAttribute("blankHeight", numericUpDownBlankHeight.Value.ToString());
        }

        void XML_Load(string path, bool isContent = false)
        {
            readImmersiveMenuSettings();

            imageList_Init();
            treeView.Nodes.Clear();

            if (!isContent)
                currentFile = path;

            XmlDocument doc = new XmlDocument();
            if (isContent)
                doc.LoadXml(path);
            else
                doc.Load(path);
            XmlNode rootNode = doc.SelectSingleNode("root");

            XML_Config_Load(rootNode);
            XML_AddMenuItems(rootNode, null);


            if (path.ToLower() == Installer.GetConfigPath().ToLower())
            {
                FormUtils.ShowShield(buttonApply.Handle, true);
            }

        }

        void XML_Save(string path)
        {
            saveImmersiveMenuSettings();

            XmlDocument doc = new XmlDocument();
            XmlElement rootNode = doc.CreateElement("root");

            XML_Config_Save(rootNode);
            XML_GenerateContent(doc, rootNode, treeView.Nodes);

            doc.AppendChild(rootNode);

            try
            {
                doc.Save(path);
            }
            catch (UnauthorizedAccessException)
            {
                string tmp = System.IO.Path.GetTempFileName();
                doc.Save(tmp);


                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Application.ExecutablePath;
                startInfo.Arguments = "CWP \"" + tmp + "\" \"" + path + "\"";
                startInfo.Verb = "runas";

                try
                {
                    System.Diagnostics.Process.Start(startInfo).WaitForExit();
                }catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show("This action requires administrative privilage!", "Fail to save the config file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void XML_AddMenuItems(XmlNode xmlNode, TreeNode parentNode)
        {
            TreeNode curTreeNode = null;
            foreach (XmlNode curXMLNode in xmlNode.ChildNodes)
            {
                if (curXMLNode.Name == "item" || curXMLNode.Name == "submenu")
                {
                    if (parentNode == null)
                    {
                        curTreeNode = treeView.Nodes.Add(curXMLNode.Attributes.GetNamedItem("text").Value);
                    }
                    else
                    {
                        curTreeNode = parentNode.Nodes.Add(curXMLNode.Attributes.GetNamedItem("text").Value);
                    }

                    curTreeNode.ImageKey = curXMLNode.Name;
                    curTreeNode.SelectedImageKey = curXMLNode.Name;
                    
                    ItemProperty tag = new ItemProperty();
                    curTreeNode.Tag = tag;
                    curTreeNode.ContextMenuStrip = contextMenuStrip;


                    tag.IconFile = XML_GetNodeAttribute(curXMLNode, "icon", null);
                    if (tag.IconFile != null)
                    {
                        tag.IconSize = XML_GetNodeAttributeInt(curXMLNode, "size", 0);
                        tag.IconIndex = XML_GetNodeAttributeInt(curXMLNode, "index", 0);

                        curTreeNode.ImageKey = imageList_AddIcon(tag.IconFile, tag.IconIndex);
                        curTreeNode.SelectedImageKey = curTreeNode.ImageKey;
                    }




                    if (curXMLNode.Name == "item")
                    {
                        tag.IsSubMenu = false;
                        string[] splited = curXMLNode.InnerText.Split('|');
                        tag.Command = splited[0];
                        if (splited.Length > 1)
                            tag.Arguments = splited[1];
                        else
                            tag.Arguments = "";
                    }
                    else if (curXMLNode.Name == "submenu")
                    {
                        tag.IsSubMenu = true;
                        XML_AddMenuItems(curXMLNode, curTreeNode);
                    }

                }
                else if (curXMLNode.Name == "separator")
                {
                    if (parentNode == null)
                    {
                        curTreeNode = treeView.Nodes.Add("Separator");
                    }
                    else
                    {
                        curTreeNode = parentNode.Nodes.Add("Separator");
                    }

                    curTreeNode.ImageKey = curXMLNode.Name;
                    curTreeNode.SelectedImageKey = curXMLNode.Name;
                    curTreeNode.Tag = null;
                    curTreeNode.ContextMenuStrip = contextMenuStrip;
                }                
            }
        }

        void XML_GenerateContent(XmlDocument curDoc, XmlElement xmlParent, TreeNodeCollection nodes)
        {
            foreach (TreeNode curNode in nodes)
            {
                if (curNode.Tag == null)
                {
                    xmlParent.AppendChild(curDoc.CreateElement("separator"));
                }
                else
                {
                    XmlElement curXMLNode;
                    ItemProperty tag = (ItemProperty)curNode.Tag;
                    if (tag.IsSubMenu)
                    {
                        curXMLNode = curDoc.CreateElement("submenu");
                    }
                    else
                    {
                        curXMLNode = curDoc.CreateElement("item");
                        curXMLNode.InnerText = "" + tag.Command + "";
                        if (tag.Arguments != "")
                            curXMLNode.InnerText += "|" + tag.Arguments + "";
                    }

                    curXMLNode.SetAttribute("text", curNode.Text);

                    if (tag.HasIcon())
                    {
                        curXMLNode.SetAttribute("icon", tag.IconFile);
                        if (tag.IconIndex > 0)
                            curXMLNode.SetAttribute("index", tag.IconIndex.ToString());
                        if (tag.HasCustomIconSize())
                            curXMLNode.SetAttribute("size", tag.IconSize.ToString());
                    }

                    if (tag.IsSubMenu)
                        XML_GenerateContent(curDoc, curXMLNode, curNode.Nodes);

                    xmlParent.AppendChild(curXMLNode);
                }
            }
        }
        #endregion

        #region ImmersiveMenu_Registry
        void readImmersiveMenuSettings()
        {
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer", false);
            int menuConfig = (int)regKey.GetValue("ContextMenuConfig", 0);

            checkBoxEnableImmersiveMenu.Checked = (menuConfig & 0x01) > 0;
            checkBoxShowIcon.Checked = (menuConfig & 0x02) > 0;
            checkBoxHideToggleOption.Checked = (menuConfig & 0x04) > 0;
        }

        void saveImmersiveMenuSettings()
        {
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer", true);
            int menuConfig = 0;

            if (checkBoxEnableImmersiveMenu.Checked)
                menuConfig |= 0x01;
            if (checkBoxShowIcon.Checked)
                menuConfig |= 0x02;
            if (checkBoxHideToggleOption.Checked)
                menuConfig |= 0x04;

            regKey.SetValue("ContextMenuConfig", menuConfig, Microsoft.Win32.RegistryValueKind.DWord);
        }

        #endregion

        #region ContextMenu

        private void contextMenuStripItem_Opening(object sender, CancelEventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                e.Cancel = true;
                return;
            }

            if (treeView.SelectedNode.Tag == null)
            {
                toolStripMenuItemCommand.Visible = false;
                toolStripMenuItemRename.Visible = false;
                toolStripMenuItemIconSettings.Visible = false;
            }
            else
            {
                foreach (ToolStripItem menuItem in contextMenuStrip.Items)
                {
                    menuItem.Visible = true;
                }

                ItemProperty tag = (ItemProperty)treeView.SelectedNode.Tag;
                toolStripMenuItemRemoveIcon.Enabled = tag.HasIcon();
                toolStripMenuItemCommand.Visible = !tag.IsSubMenu;

                if (tag.HasIcon())
                {
                    toolStripComboBoxIconSize.Enabled = true;
                    if (tag.HasCustomIconSize())
                    {
                        toolStripComboBoxIconSize.Text = tag.IconSize.ToString();
                    }
                    else
                    {
                        toolStripComboBoxIconSize.Text = toolStripComboBoxIconSize.Items[0].ToString();
                    }
                }
                else
                {
                    toolStripComboBoxIconSize.Enabled = false;
                    toolStripComboBoxIconSize.Text = "";
                }
            }

            if (treeView.SelectedNode.Parent == null)
            {
                toolStripMenuItemMoveToPrevLevel.Enabled = false;
            }
            else
            {
                toolStripMenuItemMoveToPrevLevel.Enabled = treeView.SelectedNode.Parent != null;
            }

            
            toolStripMenuItemMoveUp.Enabled = treeView.SelectedNode.PrevNode != null;
            toolStripMenuItemMoveDown.Enabled = treeView.SelectedNode.NextNode != null;
        }

        private void toolStripMenuItemMoveUp_Click(object sender, EventArgs e)
        {
            TreeNode newNode = (TreeNode)(treeView.SelectedNode.Clone());
            if (treeView.SelectedNode.Parent == null)
            {
                treeView.Nodes.Insert(treeView.SelectedNode.PrevNode.Index, newNode);
                treeView.SelectedNode.Remove();
            }
            else
            {
                treeView.SelectedNode.Parent.Nodes.Insert(treeView.SelectedNode.PrevNode.Index, newNode);
                treeView.SelectedNode.Remove();
            }
            treeView.SelectedNode = newNode;
        }

        private void toolStripMenuItemMoveDown_Click(object sender, EventArgs e)
        {
            TreeNode newNode = (TreeNode)(treeView.SelectedNode.Clone());
            if (treeView.SelectedNode.Parent == null)
            {
                treeView.Nodes.Insert(treeView.SelectedNode.NextNode.Index + 1, newNode);
                treeView.SelectedNode.Remove();
            }
            else
            {
                treeView.SelectedNode.Parent.Nodes.Insert(treeView.SelectedNode.NextNode.Index + 1, newNode);
                treeView.SelectedNode.Remove();
            }
            treeView.SelectedNode = newNode;
        }

        private void toolStripMenuItemRename_Click(object sender, EventArgs e)
        {
            treeView.SelectedNode.BeginEdit();
        }

        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            treeView.SelectedNode.Remove();
        }

        private void toolStripMenuItemCommand_Click(object sender, EventArgs e)
        {
            ItemProperty tag = (ItemProperty)treeView.SelectedNode.Tag;
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog(this) == DialogResult.Cancel)
                return;

            tag.Command = ofd.FileName;
            UpdateInfoPanel();


            //MessageBox.Show(((ItemProperty)treeView.SelectedNode.Tag).Arguments, ((ItemProperty)treeView.SelectedNode.Tag).Command);
        }

        private void toolStripMenuItemMoveToPrevLevel_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode.Parent != null)
            {
                TreeNode newNode = (TreeNode)treeView.SelectedNode.Clone();
                if (treeView.SelectedNode.Parent.Parent == null)
                    treeView.Nodes.Insert(treeView.SelectedNode.Parent.Index, newNode);
                else
                    treeView.SelectedNode.Parent.Parent.Nodes.Insert(treeView.SelectedNode.Parent.Index, newNode);

                treeView.SelectedNode.Remove();
                treeView.SelectedNode = newNode;
            }
        }

        private void toolStripMenuItemNewMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode newNode = null;

            if (treeView.SelectedNode == null)
                newNode = treeView.Nodes.Add("New menu item");
            else if (treeView.SelectedNode.Tag!=null && ((ItemProperty)treeView.SelectedNode.Tag).IsSubMenu)
                newNode = treeView.SelectedNode.Nodes.Add("New menu item");
            else
            {
                if (treeView.SelectedNode.Parent == null)
                    newNode = treeView.Nodes.Insert(treeView.SelectedNode.Index + 1, "New menu item");
                else
                    newNode = treeView.SelectedNode.Parent.Nodes.Insert(treeView.SelectedNode.Index + 1, "New menu item");
            }
                
            ItemProperty itemProperty = new ItemProperty();
            newNode.Tag = itemProperty;
            newNode.ContextMenuStrip = contextMenuStrip;
            newNode.ImageKey = "Item";
            newNode.SelectedImageKey = "Item";
            itemProperty.Command = "";
            itemProperty.Arguments = "";
            itemProperty.IconFile = null;
            itemProperty.IsSubMenu = false;
        }

        private void toolStripMenuItemNewSubMenu_Click(object sender, EventArgs e)
        {
            TreeNode newNode = null;

            if (treeView.SelectedNode == null)
                newNode = treeView.Nodes.Add("New sub menu");
            else if (treeView.SelectedNode.Tag != null && ((ItemProperty)treeView.SelectedNode.Tag).IsSubMenu)
                newNode = treeView.SelectedNode.Nodes.Add("New sub menu");
            else
            {
                if (treeView.SelectedNode.Parent == null)
                    newNode = treeView.Nodes.Insert(treeView.SelectedNode.Index + 1, "New sub menu");
                else
                    newNode = treeView.SelectedNode.Parent.Nodes.Insert(treeView.SelectedNode.Index + 1, "New sub menu");
            }

            ItemProperty itemProperty = new ItemProperty();
            newNode.Tag = itemProperty;
            newNode.ContextMenuStrip = contextMenuStrip;
            newNode.ImageKey = "SubMenu";
            newNode.SelectedImageKey = "SubMenu";
            itemProperty.Command = "";
            itemProperty.Arguments = "";
            itemProperty.IconFile = null;
            itemProperty.IsSubMenu = true;
        }

        private void toolStripMenuItemNewSeperator_Click(object sender, EventArgs e)
        {
            TreeNode newNode = null;

            if (treeView.SelectedNode == null)
                newNode = treeView.Nodes.Add("Seperator");
            else if(treeView.SelectedNode.Tag != null && ((ItemProperty)treeView.SelectedNode.Tag).IsSubMenu)
                newNode = treeView.SelectedNode.Nodes.Add("Seperator");
            else
            {
                if (treeView.SelectedNode.Parent == null)
                    newNode = treeView.Nodes.Insert(treeView.SelectedNode.Index + 1, "Seperator");
                else
                    newNode = treeView.SelectedNode.Parent.Nodes.Insert(treeView.SelectedNode.Index + 1, "Seperator");
            }

            ItemProperty itemProperty = new ItemProperty();
            newNode.Tag = null;
            newNode.ContextMenuStrip = contextMenuStrip;
            newNode.ImageKey = "Seperator";
            newNode.SelectedImageKey = "Seperator";
        }

        private void toolStripMenuItemMoveTop_Click(object sender, EventArgs e)
        {
            TreeNode newNode = (TreeNode)treeView.SelectedNode.Clone();
            if (treeView.SelectedNode.Parent == null)
            {
                treeView.Nodes.Insert(0, newNode);
            }
            else
            {
                treeView.SelectedNode.Parent.Nodes.Insert(0, newNode);
            }
            treeView.SelectedNode.Remove();
            treeView.SelectedNode = newNode;
        }

        private void toolStripMenuItemMoveBottom_Click(object sender, EventArgs e)
        {
            TreeNode newNode = (TreeNode)treeView.SelectedNode.Clone();
            if (treeView.SelectedNode.Parent == null)
            {
                treeView.Nodes.Insert(treeView.Nodes.Count, newNode);
            }
            else
            {
                treeView.SelectedNode.Parent.Nodes.Insert(treeView.SelectedNode.Parent.Nodes.Count, newNode);
            }
            treeView.SelectedNode.Remove();
            treeView.SelectedNode = newNode;
        }

        private void toolStripMenuItemBrowseIcon_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            if (treeView.SelectedNode.Tag == null)
                return;

            ItemProperty tag = (ItemProperty)treeView.SelectedNode.Tag;
            OpenIconDialog dlg = new OpenIconDialog();

            if (tag.HasIcon())
                dlg.IconFile = tag.IconFile;
            else
                dlg.IconFile = tag.Command;

            dlg.IconIndex = tag.IconIndex;
            if (dlg.ShowDialog(this.Handle) == DialogResult.Cancel)
                return;

            tag.IconFile = dlg.IconFile;
            tag.IconIndex = dlg.IconIndex;

            

            //Icon selectedIcon = IconManager.GetIcon(dlg.IconFile, dlg.IconIndex, false);
            //imageList.Images.Add(selectedIcon);
            //selectedIcon.Dispose();
            treeView.SelectedNode.ImageKey = imageList_AddIcon(tag.IconFile, tag.IconIndex);
            treeView.SelectedNode.SelectedImageKey = treeView.SelectedNode.ImageKey;

            UpdateInfoPanel();
        }


        private void toolStripMenuItemRemoveIcon_Click(object sender, EventArgs e)
        {
            ItemProperty tag = (ItemProperty)treeView.SelectedNode.Tag;
            tag.IconFile = null;
            tag.IconIndex = 0;
            tag.IconSize = 0;

            treeView.SelectedNode.ImageKey = tag.IsSubMenu ? "SubMenu" : "Item";
            treeView.SelectedNode.SelectedImageKey = treeView.SelectedNode.ImageKey;

            UpdateInfoPanel();
        }

        private void toolStripComboBoxIconSize_DropDownClosed(object sender, EventArgs e)
        {
            ItemProperty tag = (ItemProperty)treeView.SelectedNode.Tag;
            if (toolStripComboBoxIconSize.SelectedIndex == 0)
            {
                tag.IconSize = 0;
            }
            else
            {
                tag.IconSize = int.Parse(toolStripComboBoxIconSize.Text);
            }
        }
        #endregion


        void UpdateInfoPanel()
        {
            UpdateInfoPanel(treeView.SelectedNode);
        }

        void UpdateInfoPanel(TreeNode selectedTreeNode)
        {
            labelName.Text = selectedTreeNode.Text + ":";

            if (selectedTreeNode.Tag != null)
            {
                ItemProperty tag = (ItemProperty)selectedTreeNode.Tag;

                Icon ico = IconManager.GetIcon(tag.IconFile, tag.IconIndex, true);
                if (ico != null)
                {
                    pictureBoxIcon.Image = ico.ToBitmap();
                    ico.Dispose();
                }
                else
                {
                    pictureBoxIcon.Image = null;
                }

                if (!tag.IsSubMenu)
                {
                    textBoxCommand.Text = tag.Command;
                    textBoxArguments.Text = tag.Arguments;
                }
                else
                {
                    textBoxCommand.Text = "";
                    textBoxArguments.Text = "";
                }
            }
            else
            {
                textBoxCommand.Text = "";
                textBoxArguments.Text = "";
                pictureBoxIcon.Image = null;
            }
        }


        #region TreeView
        private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node.Tag == null)
                e.CancelEdit = true;
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateInfoPanel();
        }

        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            UpdateInfoPanel();
        }

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            treeView.SelectedNode = treeView.GetNodeAt(new Point(e.X, e.Y));
        }

        private TreeNode dragNode = null;
        //private TreeNode tempDropNode = null;
        private void treeView_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
        {
            // Get drag node and select it
            this.dragNode = (TreeNode)e.Item;
            this.treeView.SelectedNode = this.dragNode;

            // Reset image list used for drag image
            this.imageListDrag.Images.Clear();
            this.imageListDrag.ImageSize = new Size(this.dragNode.Bounds.Size.Width + this.treeView.Indent, this.dragNode.Bounds.Height);

            // Create new bitmap
            // This bitmap will contain the tree node image to be dragged
            Bitmap bmp = new Bitmap(this.dragNode.Bounds.Width + this.treeView.Indent, this.dragNode.Bounds.Height);

            // Get graphics from bitmap
            Graphics gfx = Graphics.FromImage(bmp);

            // Draw node icon into the bitmap
            gfx.DrawImage(this.imageList.Images[this.dragNode.ImageKey], 0, 0);

            // Draw node label into bitmap
            gfx.DrawString(this.dragNode.Text,
                this.treeView.Font,
                new SolidBrush(this.treeView.ForeColor),
                (float)this.treeView.Indent, 1.0f);

            // Add bitmap to imagelist
            this.imageListDrag.Images.Add(bmp);

            // Get mouse position in client coordinates
            Point p = this.treeView.PointToClient(Control.MousePosition);

            // Compute delta between mouse position and node bounds
            int dx = p.X + this.treeView.Indent - this.dragNode.Bounds.Left;
            int dy = p.Y - this.dragNode.Bounds.Top;

            // Begin dragging image
            if (WinAPI.ImageList_BeginDrag(this.imageListDrag.Handle, 0, dx, dy))
            {
                // Begin dragging
                this.treeView.DoDragDrop(bmp, DragDropEffects.Move);
                // End dragging image
                WinAPI.ImageList_EndDrag();
            }

        }

        private void treeView_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Compute drag position and move image
            Point formP = this.treeView.Parent.PointToClient(new Point(e.X, e.Y));
            WinAPI.ImageList_DragMove(formP.X - this.treeView.Left, formP.Y - this.treeView.Top);

            // Get actual drop node
            TreeNode dropNode = this.treeView.GetNodeAt(this.treeView.PointToClient(new Point(e.X, e.Y)));
            if (dropNode == null || dropNode.Tag == null || !((ItemProperty)dropNode.Tag).IsSubMenu)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Move;

            // if mouse is on a new node select it
            //if (this.tempDropNode != dropNode)
            //{
                //DragHelper.ImageList_DragShowNolock(false);
                //this.treeView.SelectedNode = dropNode;
                //DragHelper.ImageList_DragShowNolock(true);
             //   tempDropNode = dropNode;
            //}

            // Avoid that drop node is child of drag node 
            TreeNode tmpNode = dropNode;
            while (tmpNode.Parent != null)
            {
                if (tmpNode.Parent == this.dragNode) e.Effect = DragDropEffects.None;
                tmpNode = tmpNode.Parent;
            }
        }

        private void treeView_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Unlock updates
            WinAPI.ImageList_DragLeave(this.treeView.Handle);

            // Get drop node
            TreeNode dropNode = this.treeView.GetNodeAt(this.treeView.PointToClient(new Point(e.X, e.Y)));

            // If drop node isn't equal to drag node, add drag node as child of drop node
            if (this.dragNode != dropNode)
            {
                if (dragNode.Parent == dropNode)
                    return;
    
                TreeNode newNode = (TreeNode)this.dragNode.Clone();


                // Remove drag node from parent
                if (this.dragNode.Parent == null)
                {
                    this.treeView.Nodes.Remove(this.dragNode);
                }
                else
                {
                    this.dragNode.Parent.Nodes.Remove(this.dragNode);
                }

                // Add drag node to drop node
                dropNode.Nodes.Insert(0,newNode);
                dropNode.ExpandAll();
                treeView.SelectedNode = newNode;

                // Set drag node to null
                this.dragNode = null;

                // Disable scroll timer
                //this.timer.Enabled = false;
            }
        }

        private void treeView_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            WinAPI.ImageList_DragEnter(this.treeView.Handle, e.X - this.treeView.Left,
                e.Y - this.treeView.Top);

            // Enable timer for scrolling dragged item
            //this.timer.Enabled = true;
        }

        private void treeView_DragLeave(object sender, System.EventArgs e)
        {
            WinAPI.ImageList_DragLeave(this.treeView.Handle);

            // Disable timer for scrolling dragged item
            //this.timer.Enabled = false;
        }

        private void treeView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effect == DragDropEffects.Move)
            {
                // Show pointer cursor while dragging
                e.UseDefaultCursors = false;
                this.treeView.Cursor = Cursors.Default;
            }
            else e.UseDefaultCursors = true;

        }
        #endregion

        #region ConfigTab
        private void comboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupBoxCaption.Enabled = comboBoxStyle.SelectedIndex == 2 || comboBoxStyle.SelectedIndex == 4;
            numericUpDownBlankWidth.Enabled = comboBoxStyle.SelectedIndex > 0;
            numericUpDownBlankHeight.Enabled = comboBoxStyle.SelectedIndex > 0;
        }

        private void comboBoxSysVer_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string param = MenuControl.GetInjectParamFromIndex(comboBoxSysVer.SelectedIndex);
            if (param != null)
                textBoxClassName.Text = param;
        }

        private void buttonCommand_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            ItemProperty tag = (ItemProperty)treeView.SelectedNode.Tag;
            tag.Command = textBoxCommand.Text;
        }

        private void buttonArguments_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            ItemProperty tag = (ItemProperty)treeView.SelectedNode.Tag;
            tag.Arguments = textBoxArguments.Text;
        }


        #endregion

        #region install
        private void buttonUpdateStartupCommand_Click(object sender, EventArgs e)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Application.ExecutablePath;
            startInfo.Arguments = "STARTUP \"" + textBoxClassName.Text + "\"";
            startInfo.Verb = "runas";

            try
            {
                System.Diagnostics.Process.Start(startInfo).WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("This action requires administrative privilage!", "Fail to save config", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonRemoveStartUp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Application.ExecutablePath;
            startInfo.Arguments = "STARTUP \"?\"";
            startInfo.Verb = "runas";

            try
            {
                System.Diagnostics.Process.Start(startInfo).WaitForExit();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("This action requires administrative privilage!", "Fail to save config", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonStartNow_Click(object sender, EventArgs e)
        {
            if (MenuControl.isMenu98ModuleLoaded())
                labelStatus.Text = "Status: Loaded";
            else
            {
                MenuControl.Menu98_Inject(textBoxClassName.Text);
                buttonDetect_Click(sender, e);
            }
        }

        private void buttonReleadDLL_Click(object sender, EventArgs e)
        {
            MenuControl.Menu98_ExitHost();
            buttonDetect_Click(sender, e);
        }

        private void buttonDetect_Click(object sender, EventArgs e)
        {
            if (MenuControl.isMenu98ModuleLoaded())
                labelStatus.Text = "Status: Loaded";
            else
                labelStatus.Text = "Status: Not loaded";
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            Installer.ShowInstaller(this);
        }

        private void buttonUninstall_Click(object sender, EventArgs e)
        {
            Installer.LaunchUninstall(this);
        }
        #endregion

        private void buttonApply_Click(object sender, EventArgs e)
        {
            XML_Save(currentFile);
            MenuControl.Menu98_ExitHost();
            MenuControl.Menu98_Inject(textBoxClassName.Text);

            if (Installer.ExplorerImmersiveMenuRemoved() != checkBoxNoExplorerImmersiveMenu.Checked)
                Installer.ExplorerImmersiveMenuUsing(!checkBoxNoExplorerImmersiveMenu.Checked);
        }


        private void buttonLoadSysCfg_Click(object sender, EventArgs e)
        {
            XML_Load(Installer.GetConfigPath());
        }

        private void ToolStripButtonHelp_Click(object sender, EventArgs e)
        {
            WinVer.ShowShellAboutBox(this, labelCaption.Text, 
                "Author: Rikka0w0 (Github) \n" +
                "Some tweaks come from askvg.com");
        }

        private void ToolStripButtonOpen_Click(object sender, EventArgs e)
        {
            OpenConfigUsingDialog();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            XML_Save(currentFile);
        }
    }
}

