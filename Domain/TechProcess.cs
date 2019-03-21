﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CAM.Domain
{
    /// <summary>
    /// Технологический процесс обработки
    /// </summary>
    public class TechProcess
    {
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Параметры технологического процесса
        /// </summary>
        public TechProcessParams TechProcessParams { get; set; }

        public List<ITechOperationFactory> TechOperationFactorys { get; set; } = new List<ITechOperationFactory>();

        /// <summary>
        /// Список технологических операций процесса
        /// </summary>
        public List<TechOperation> TechOperations { get; } = new List<TechOperation>();

	    public IEnumerable<Curve> ToolpathCurves => TechOperations.SelectMany(p => p.ToolpathCurves);

        /// <summary>
        /// Команды
        /// </summary>
        public List<ProcessCommand> ProcessCommands { get; set; } = new List<ProcessCommand>();

        public TechProcess(string name)
        {
            Name = name;
            TechProcessParams = new TechProcessParams();
        }

	    public void BuildProcessing()
	    {
			BorderProcessingArea.SetupBorders(TechOperations.Select(p => p.ProcessingArea).OfType<BorderProcessingArea>().ToList());
			var currentPoint = Point3d.Origin;
		    TechOperations.ForEach(p => currentPoint = p.BuildProcessing(currentPoint, p == TechOperations.Last()));
	        ProcessCommands = TechOperations.SelectMany(p => p.ProcessCommands).ToList();
	    }
	}
}