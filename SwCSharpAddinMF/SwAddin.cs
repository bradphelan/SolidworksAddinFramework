using System.Linq;
using System.Runtime.InteropServices;
using SolidworksAddinFramework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorksTools;


namespace SwCSharpAddinMF
{
    /// <summary>
    /// Summary description for SwCSharpAddinMF.
    /// </summary>
    [Guid("7612e834-6277-4122-9e8f-675258162910"), ComVisible(true)]
    [SwAddin(
        Description = "SwCSharpAddinMF description",
        Title = "SwCSharpAddinMF",
        LoadAtStartup = true
        )]
    public class SwAddin : SwAddinBase
    {
        #region Local Variables

        public const int MainCmdGroupId = 5;
        public const int MainItemId1 = 0;
        public const int MainItemId2 = 1;
        public const int MainItemId3 = 2;

        #endregion


        #region UI Methods
        public override void AddCommandMgr()
        {
            const string title = "C# Addin";
            const string toolTip = "C# Addin";

            int[] docTypes = { (int)swDocumentTypes_e.swDocPART};

            var cmdGroupErr = 0;
            var ignorePrevious = false;

            object registryIDs;
            //get the ID information stored in the registry
            var getDataResult = CommandManager.GetGroupDataFromRegistry(MainCmdGroupId, out registryIDs);

            int[] knownIDs = { MainItemId1, MainItemId2 };

            if (getDataResult)
            {
                if (!CompareIDs((int[])registryIDs, knownIDs)) //if the IDs don't match, reset the commandGroup
                {
                    ignorePrevious = true;
                }
            }

            ICommandGroup cmdGroup = CommandManager.CreateCommandGroup2(MainCmdGroupId, title, toolTip, "", -1, ignorePrevious, ref cmdGroupErr);
            cmdGroup.LargeIconList = GetBitMap("SwCSharpAddinMF.Icons.ToolbarLarge.bmp");
            cmdGroup.SmallIconList = GetBitMap("SwCSharpAddinMF.Icons.ToolbarSmall.bmp");
            cmdGroup.LargeMainIcon = GetBitMap("SwCSharpAddinMF.Icons.MainIconLarge.bmp");
            cmdGroup.SmallMainIcon = GetBitMap("SwCSharpAddinMF.Icons.MainIconSmall.bmp");

            var menuToolbarOption = (int)swCommandItemType_e.swToolbarItem | (int)swCommandItemType_e.swMenuItem;
            var cmdIndex0 = cmdGroup.AddCommandItem2(nameof(CreateSampleMacroFeature), -1, "Alpha Split", "Alpha Split", 0, nameof(CreateSampleMacroFeature), "", MainItemId1, menuToolbarOption);

            cmdGroup.HasToolbar = true;
            cmdGroup.HasMenu = true;
            cmdGroup.Activate();


            foreach (var type in docTypes)
            {
                var cmdTab = CommandManager.GetCommandTab(type, title);

                // if tab exists, but we have ignored the registry info (or changed command group ID), re-create the tab.
                // Otherwise the ids won't matchup and the tab will be blank
                if (cmdTab != null & !getDataResult | ignorePrevious)
                {
                    CommandManager.RemoveCommandTab(cmdTab);
                    cmdTab = null;
                }

                //if cmdTab is null, must be first load (possibly after reset), add the commands to the tabs
                if (cmdTab == null)
                {
                    cmdTab = CommandManager.AddCommandTab(type, title);

                    var cmdBox = cmdTab.AddCommandTabBox();

                    var cmdIDs = new[] {cmdIndex0}
                        .Select(id => cmdGroup.CommandID[id])
                        .ToArray();

                    var textType = cmdIDs
                        .Select(id => (int) swCommandTabButtonTextDisplay_e.swCommandTabButton_TextHorizontal)
                        .ToArray();

                        
                    cmdBox.AddCommands(cmdIDs, textType);

                }

            }
        }

        public override void RemoveCommandMgr()
        {
            CommandManager.RemoveCommandGroup(MainCmdGroupId);
        }

        #endregion

        #region UI Callbacks
        public void CreateSampleMacroFeature()
        {
            SampleMacroFeature.AddMacroFeature(SwApp);
        }


        #endregion


    }

}
