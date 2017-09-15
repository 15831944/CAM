﻿using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAM.Domain
{
    /// <summary>
    /// Распиловка по дуге
    /// </summary>
    public class SawingArcTechOperation : SawingTechOperation
    {
        /// <summary>
        /// Обрабатываемая область
        /// </summary>
        public ArcProcessingArea processingArea { get; }

        public override ProcessingArea GetProcessingArea() => processingArea;

        public SawingArcTechOperation(TechProcessParams techProcessParams, SawingTechOperationParams techOperationParams, Arc arc)
             : base(techProcessParams, techOperationParams)
        {
            processingArea = new ArcProcessingArea(arc);
            Name = "Распил по дуге";
        }
    }
}