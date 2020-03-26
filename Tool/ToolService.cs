﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CAM
{
    public static class ToolService
    {
        private static ToolsForm _toolsForm;
        private static Dictionary<MachineType, MachineSettings> _machineSettings;
        private static Dictionary<MachineType, List<Tool>> _tools = new Dictionary<MachineType, List<Tool>>();

        public static void SetMachineSettings(Dictionary<MachineType, MachineSettings> machineSettings) => _machineSettings = machineSettings;

        public static void AddMachineTools(MachineType machineType, List<Tool> tools) => _tools.Add(machineType, tools);

        public static Tool SelectTool(MachineType machineType)
        {
            if (_toolsForm == null)
                _toolsForm = new ToolsForm();
            _toolsForm.Text = $"Инструмент {machineType}";
            _toolsForm.ToolBindingSource.DataSource = _tools[machineType];

            if (Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(_toolsForm) == DialogResult.OK)
                return (Tool)_toolsForm.ToolBindingSource.Current;
            return null;
        }

        public static int CalcFrequency(Tool tool, MachineType machineType, Material material)
        {
            var speed = material == Material.Granite ? 35 : 50;
            var frequency = (int)Math.Round(speed * 1000 / (tool.Diameter * Math.PI) * 60);
            return Math.Min(frequency, _machineSettings[machineType].MaxFrequency);
        }
    }
}
