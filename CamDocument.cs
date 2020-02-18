﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Dreambuild.AutoCAD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CAM
{
    public class CamDocument
    {
        public readonly Document Document;
        public int Hash;
        public List<ITechProcess> TechProcessList { get; set; } = new List<ITechProcess>();
        private Settings _settings;
        private readonly TechProcessFactory _techProcessFactory;

        public CamDocument(Document document, Settings settings)
        {
            Document = document;
            _settings = settings;
            _techProcessFactory = new TechProcessFactory(settings);
        }

        public ITechProcess CreateTechProcess(string techProcessName)
        {
            var techProcess = _techProcessFactory.CreateTechProcess(techProcessName);
            TechProcessList.Add(techProcess);
            return techProcess;
        }

        public List<ITechOperation> CreateTechOperation(ITechProcess techProcess, string techOperationName) => _techProcessFactory.CreateTechOperations(techProcess, techOperationName);

        public IEnumerable<string> GetTechProcessNames() => _techProcessFactory.GetTechProcessNames();

        public ILookup<Type, string> GetTechOperationNames() => _techProcessFactory.GetTechOperationNames();

        public void DeleteTechProcess(ITechProcess techProcess)
        {
            Acad.DeleteExtraObjects(techProcess.ToolpathCurves, techProcess.ToolModel);
            TechProcessList.Remove(techProcess);
        }

        public void DeleteTechOperation(TechOperationBase techOperation)
        {
            Acad.DeleteCurves(techOperation.ToolpathCurves);
            techOperation.TechProcess.TechOperations.Remove(techOperation);
        }

        public void SelectTechProcess(ITechProcess techProcess) => Acad.SelectObjectIds(techProcess.ProcessingArea?.AcadObjectIds);

        public void SelectTechOperation(ITechOperation techOperation) {} // Acad.SelectObjectIds(techOperation.ProcessingArea.AcadObjectIds.ToArray());

        public void SelectProcessCommand(ITechProcess techProcess, ProcessCommand processCommand)
        {
            Acad.SelectCurve(processCommand.ToolpathCurve);
            if (techProcess.ToolModel == null && processCommand.EndPoint != null)
                techProcess.ToolModel = Acad.CreateToolModel(techProcess.Tool.Diameter, techProcess.Tool.Thickness.Value);
            Acad.DrawToolModel(techProcess.ToolModel, processCommand.EndPoint, processCommand.ToolAngle);
        }
       
        public void BuildProcessing(ITechProcess techProcess, BorderProcessingArea startBorder = null)
        {
            try
            {
                Acad.Write($"Выполняется расчет обработки по техпроцессу {techProcess.Caption} ...");

                Acad.DeleteExtraObjects(techProcess.ToolpathCurves, techProcess.ToolModel);
                techProcess.ToolModel = null;

                techProcess.BuildProcessing(); // startBorder);

                Acad.SaveCurves(techProcess.ToolpathCurves);

                Acad.Write("Расчет обработки завершен");
            }
            catch (Exception ex)
            {
                techProcess.DeleteProcessCommands();
                Acad.Alert("Ошибка при выполнении расчета", ex);
            }
        }

        public void HideShowProcessing(ITechProcess techProcess)
        {
            Acad.HideExtraObjects(techProcess.ToolpathCurves, techProcess.ToolModel);
            techProcess.ToolModel = null;
        }

        public void SwapOuterSide(ITechProcess techProcess, TechOperationBase techOperation)
        {
            //var to = techOperation ?? techProcess?.TechOperations?.FirstOrDefault();
            //if (to?.ProcessingArea is BorderProcessingArea border)
            //{
            //    border.OuterSide = border.OuterSide.Swap();
            //    BuildProcessing(to.TechProcess, border);
            //}
        }

        public void SendProgram(List<ProcessCommand> processCommands, ITechProcess techProcess)
        {
            if (processCommands == null || !processCommands.Any())
            {
                Acad.Alert("Программа не сформирована");
                return;
            }
            var fileName = Acad.SaveFileDialog(techProcess.Caption, techProcess.MachineSettings.ProgramFileExtension, techProcess.MachineType.ToString());
            if (fileName != null)
                try
                {
                    var contents = processCommands?.Select(p => p.GetProgrammLine()).ToArray();
                    File.WriteAllLines(fileName, contents);
                    Acad.Write($"Создан файл {fileName}");
                }
                catch (Exception ex)
                {
                    Acad.Alert($"Ошибка при записи файла {fileName}", ex);
                }
        }
    }
}
