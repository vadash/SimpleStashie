using ExileCore;
using ExileCore.Shared.Enums;
using ImGuiNET;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleStashie
{
    public class SimpleStashie : BaseSettingsPlugin<SimpleStashieSettings>
    {
        private Stopwatch Timer { get; } = new Stopwatch();
        private Random Random { get; } = new Random();
        private Vector2 ClickWindowOffset => GameController.Window.GetWindowRectangle().TopLeft;

        private static bool IsRunning { get; set; } = false;


        public override bool Initialise()
        {
            Timer.Start();
            Input.RegisterKey(Keys.LControlKey);
            return true;
        }

        public override Job Tick()
        {
            if (!Input.GetKeyState(Settings.StashItKey.Value)) return null;
            if (!GameController.Window.IsForeground()) return null;
            if (!GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible) return null;
            if (!GameController.Game.IngameState.IngameUi.StashElement.IsVisibleLocal) return null;
            if (IsRunning) return null;

            IsRunning = true;

            return new Job("SimpleStashie", StashItemsWithRetries);
        }

        public void StashItemsWithRetries()
        {
            for (var i = 0; i < Settings.AmountOfRetries.Value; i++)
            {
                var stashTask = Task.Run(StashItems);
                stashTask.Wait();
                Thread.Sleep(Settings.WaitTimeInMs - 10 + Random.Next(0, 20));
            }
            IsRunning = false;
        }

        private void StashItems()
        {
            var items = GameController.Game.IngameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory]?.VisibleInventoryItems;
            if (items == null)
            {
                DebugWindow.LogError("Items in inventory is null.");
                return;
            }

            try
            {
                Input.KeyDown(Keys.LControlKey);
                foreach (var item in items)
                {
                    if (SlotIsIgnored(item.InventPosX, item.InventPosY)) continue;
                    var centerOfItem = item.GetClientRect().Center
                        + ClickWindowOffset
                        + new Vector2(Random.Next(0, 5), Random.Next(0, 5));

                    Input.SetCursorPos(centerOfItem);

                    Input.Click(MouseButtons.Left);
                    Thread.Sleep(15);
                    Input.Click(MouseButtons.Left);

                    Thread.Sleep(
                        Settings.WaitTimeInMs 
                        - 15 // Sleep between clicks
                        - 10 
                        + Random.Next(0, 20)
                        );
                }
            }
            finally
            {
                Input.KeyUp(Keys.LControlKey);
            }
        }

        private bool SlotIsIgnored(int x, int y)
        {
            if (y >= 5 || x >= 12) return true;

            return Settings.IgnoredCells[y, x] == 1;
        }

        public override void DrawSettings()
        {
            DrawIgnoredCellsSettings();
            base.DrawSettings();
        }

        private void DrawIgnoredCellsSettings()
        {
            try
            {
                ImGui.SameLine();
                ImGui.TextDisabled("(?)");
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(
                        $"Checked = Item will be ignored{Environment.NewLine}UnChecked = Item will be processed");
            }
            catch (Exception e)
            {
                LogError(e.ToString(), 10);
            }

            var counter = 1;
            for (var row = 0; row < 5; row++)
            {
                for (var column = 0; column < 12; column++)
                {
                    var toggled = Convert.ToBoolean(Settings.IgnoredCells[row, column]);
                    if (ImGui.Checkbox($"##{counter}IgnoredCells", ref toggled)) Settings.IgnoredCells[row, column] ^= 1;

                    if ((counter - 1) % 12 < 11) ImGui.SameLine();

                    counter += 1;
                }
            }
        }
    }
}
