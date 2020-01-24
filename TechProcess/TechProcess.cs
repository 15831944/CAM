﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Dreambuild.AutoCAD;

namespace CAM
{
    /// <summary>
    /// Технологический процесс обработки
    /// </summary>
    [Serializable]
    public class TechProcess
    {
        //private readonly TechOperationFactory _techOperationFactory;

        [NonSerialized]
        private Settings _settings;

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Параметры технологического процесса
        /// </summary>
        public TechProcessParams TechProcessParams { get; set; }

        /// <summary>
        /// Список технологических операций процесса
        /// </summary>
        public List<ITechOperation> TechOperations { get; } = new List<ITechOperation>();

        /// <summary>
        /// Команды
        /// </summary>
        [NonSerialized]
        public List<ProcessCommand> ProcessCommands;

        [NonSerialized]
        public ToolModel ToolModel;

        public ProcessingType? ProcessingType => _techOperationFactory?.ProcessingType;

        public object TechOperationParams => _techOperationFactory.GetTechOperationParams();

        private ITechOperationFactory _techOperationFactory;

        public TechProcess(string name, Settings settings)
        {
            Name = name ?? throw new ArgumentNullException("TechProcessName");
            _settings = settings;
            TechProcessParams = _settings.TechProcessParams.Clone();
        }

        public void SetProcessingType(ProcessingType? processingType)
        {
            _techOperationFactory = processingType != null ? TechOperationFactoryProvider.CreateFactory(processingType.Value, _settings) : null;
        }

        public void Init(Settings settings)
        {
            _settings = settings;
            TechOperations.ForEach(to =>
            {
                to.ProcessingArea.AcadObjectIds = Acad.GetObjectIds(to.ProcessingArea.Handles);
                to.TechProcess = this;
            });
        }

        public IEnumerable<Curve> ToolpathCurves => TechOperations.Where(p => p.ToolpathCurves != null).SelectMany(p => p.ToolpathCurves);

        public void DeleteProcessCommands()
        {
            TechOperations.ForEach(p => p.ProcessCommands = null);
            ProcessCommands = null;
        }

        public bool SetTool(string text)
        {
            var tool = _settings.Tools.SingleOrDefault(p => p.Number.ToString() == text);
            if (tool != null)
            {
                TechProcessParams.ToolDiameter = tool.Diameter;
                TechProcessParams.ToolThickness = tool.Thickness;
                var speed = TechProcessParams.Material == Material.Гранит ? 35 : 50;
                TechProcessParams.Frequency = (int)Math.Round(speed * 1000 / (tool.Diameter * Math.PI) * 60);
            }
            return tool != null;
        }

        public ITechOperation[] CreateTechOperations(ProcessingType type, Curve[] curves)
        {
            if (_techOperationFactory == null)
            {
                Acad.Alert("Укажите вид обработки");
                return null;
            }
            return _techOperationFactory.Create(this, curves);
        }

        public bool TechOperationMoveDown(ITechOperation techOperation) => TechOperations.SwapNext(techOperation);

        public bool TechOperationMoveUp(ITechOperation techOperation) => TechOperations.SwapPrev(techOperation);

        public void BuildProcessing(BorderProcessingArea startBorder = null)
        {
            DeleteProcessCommands();
            TechOperations.ForEach(p => p.ProcessingArea.Curves = p.ProcessingArea.AcadObjectIds.QOpenForRead<Curve>());
            BorderProcessingArea.ProcessBorders(TechOperations.Select(p => p.ProcessingArea).OfType<BorderProcessingArea>().ToList(), startBorder);

            var builder = new ScemaLogicProcessBuilder(TechProcessParams);
            TechOperations.ForEach(p => p.BuildProcessing(builder));
            ProcessCommands = builder.FinishTechProcess();
        }
    }
}