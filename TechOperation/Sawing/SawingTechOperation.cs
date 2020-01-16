﻿using System;
using System.Collections.Generic;

namespace CAM.TechOperation.Sawing
{
    /// <summary>
    /// Технологическая операция "Распиловка"
    /// </summary>
    [Serializable]
    public class SawingTechOperation : TechOperationBase
    {
        /// <summary>
        /// Вид технологической операции
        /// </summary>
        public override ProcessingType Type => ProcessingType.Sawing;

        /// <summary>
        /// Параметры технологической операции
        /// </summary>
        public SawingTechOperationParams SawingParams { get; }

        public override object Params => SawingParams;

        public SawingTechOperation(TechProcess techProcess, ProcessingArea processingArea, SawingTechOperationParams sawingParams, string name)
            : base(techProcess, processingArea, name)
        {
            SawingParams = sawingParams;
        }

        public override List<CuttingSet> GetCutting()
        {
            var par = new CuttingParams
            {
                Curve = ProcessingArea.Curves[0],
                CuttingModes = SawingParams.Modes.ConvertAll(p => new CuttingMode { Depth = p.Depth, DepthStep = p.DepthStep, Feed = p.Feed}),
                IsZeroPass = SawingParams.IsFirstPassOnSurface,
                IsExactlyBegin = ((BorderProcessingArea)ProcessingArea).IsExactlyBegin,
                IsExactlyEnd = ((BorderProcessingArea)ProcessingArea).IsExactlyEnd,
                ToolSide = ((BorderProcessingArea)ProcessingArea).OuterSide,
                DepthAll = TechProcess.TechProcessParams.BilletThickness
            };
            return new List<CuttingSet> { ScemaLogicProcessBuilder.CalcCuttingSet(par, TechProcess.TechProcessParams) };
        }
    }
}
