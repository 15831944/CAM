﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Dreambuild.AutoCAD;
using System;
using System.Linq;

namespace CAM.Tactile
{
    [Serializable]
    [TechOperation(TechProcessNames.Tactile, "Фаска")]
    public class ChamfersTechOperation : TechOperationBase
    {
        public int ProcessingAngle { get; set; }

        public int CuttingFeed { get; set; }

        public ChamfersTechOperation(TactileTechProcess techProcess, string caption) : this(techProcess, caption, 0) { }

        public ChamfersTechOperation(TactileTechProcess techProcess, string caption, int processingAngle) : base(techProcess, caption)
        {
            ProcessingAngle = processingAngle;
            CuttingFeed = techProcess.TactileTechProcessParams.CuttingFeed;
        }

        public override void BuildProcessing(ScemaLogicProcessBuilder builder)
        {
            var tactileTechProcess = (TactileTechProcess)TechProcess;
            var contour = tactileTechProcess.GetContour();
            var contourPoints = contour.GetPolyPoints().ToArray();
            var ray = new Ray
            {
                BasePoint = ProcessingAngle == 45 ? contourPoints.Last() : contourPoints.First(),
                UnitDir = Vector3d.XAxis.RotateBy(Graph.ToRad(ProcessingAngle), Vector3d.ZAxis)
            };
            var passDir = ray.UnitDir.GetPerpendicularVector();
            if (ProcessingAngle >= 90)
                passDir = passDir.Negate();
            ray.BasePoint += passDir * tactileTechProcess.BandStart1.Value;
            var step = passDir * (tactileTechProcess.BandWidth.Value + tactileTechProcess.BandSpacing.Value);

            bool flag = true;
            var tactileParams = tactileTechProcess.TactileTechProcessParams;

            builder.StartTechOperation();
            while(true)
            {
                var points = new Point3dCollection();
                ray.IntersectWith(contour, Intersect.ExtendThis, new Plane(), points, IntPtr.Zero, IntPtr.Zero);
                if (points.Count == 2 && !points[0].IsEqualTo(points[1]))
                {
                    var vector = (points[1] - points[0]).GetNormal() * tactileParams.Departure;
                    var startPoint = points[0] - vector - Vector3d.ZAxis * tactileParams.Depth;
                    var endPoint = points[1] + vector - Vector3d.ZAxis * tactileParams.Depth;
                    builder.Cutting(startPoint, endPoint, CuttingFeed, tactileParams.TransitionFeed, 45);
                }
                else if (step.IsCodirectionalTo(passDir))
                {
                    ray.BasePoint -= passDir * tactileTechProcess.BandSpacing.Value;
                    step = step.Negate();
                }
                else
                    break;
                ray.BasePoint += step;
            }
            ProcessCommands = builder.FinishTechOperation();
        }
    }
}