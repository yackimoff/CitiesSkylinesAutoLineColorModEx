﻿using Priority_Queue;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LineEndpointTester
{
    public partial class Form1 : Form
    {
        private static Random InitRNG() => new Random(0);

        private readonly List<PointF> points = new List<PointF>();
        private Random rng = InitRNG();
        private int endpoint1, endpoint2;
        private PointF[] endpointPath;
        private PointF[] pointsForDrawing;
        private bool[] pointIsSynthetic;

        public Form1()
        {
            InitializeComponent();
            OnPointsChanged();
        }

        private void OnPointsChanged()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            FindEndpoints();
            stopwatch.Stop();

            var pointsForDrawingTxt =
                pointsForDrawing != null && pointsForDrawing.Length != 0 && pointsForDrawing.Length - 1 != points.Count
                ? $" ({pointsForDrawing.Length - 1})"
                : "";

            Text = $"Endpoint Tester [N={points.Count}{pointsForDrawingTxt}, t={stopwatch.ElapsedMilliseconds} ms]";

            Invalidate();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    points.Add(e.Location);
                    OnPointsChanged();
                    break;

                case MouseButtons.Right:
                    if (points.Count > 0)
                    {
                        points.RemoveAt(points.Count - 1);
                        OnPointsChanged();
                    }
                    break;
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (points.Count > 0)
            {
                e.Graphics.DrawLines(Pens.Black, pointsForDrawing);

                const float MarkSize = 7.5f;

                //System.Diagnostics.Debug.Print("endpoints: {0} and {1}", endpoint1, endpoint2);

                int oi = 0;

                for (int i = 0; i < pointsForDrawing.Length - 1; i++)
                {
                    Color color;

                    if (pointIsSynthetic[i])
                    {
                        color = Color.LightGray;
                    }
                    else
                    {
                        if (oi == endpoint1)
                            color = Color.Green;
                        else if (oi == endpoint2)
                            color = Color.Red;
                        else
                            color = Color.Black;

                        oi++;
                    }

                    using (var brush = new SolidBrush(color))
                    {
                        e.Graphics.FillEllipse(
                            brush,
                            pointsForDrawing[i].X - MarkSize / 2,
                            pointsForDrawing[i].Y - MarkSize / 2,
                            MarkSize,
                            MarkSize);
                    }
                }

                if (endpointPath != null && endpointPath.Length >= 2)
                    e.Graphics.DrawLines(Pens.Yellow, endpointPath);
            }
        }

        private void FindEndpoints()
        {
            //var metric = new TravelDistanceMetric(points);
            var metric = new InteriorDistanceMetric(points);

            int bestPoint1 = -1, bestPoint2 = -1;
            float bestDist = float.MinValue;
            PointF[] bestPath = null;

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = (i + 1) % points.Count; j != i; j = (j + 1) % points.Count)
                {
                    var dist = metric.GetDistanceBetweenPoints(i, j);

                    if (dist > bestDist)
                    {
                        bestPoint1 = i;
                        bestPoint2 = j;
                        bestDist = dist;
                        bestPath = metric.GetPathBetweenPoints(i, j).ToArray();
                    }
                }
            }

            endpoint1 = bestPoint1;
            endpoint2 = bestPoint2;
            endpointPath = bestPath;
            pointsForDrawing = metric.GetPointsForDrawing(out pointIsSynthetic);
        }

        private void recalcButton_Click(object sender, EventArgs e)
        {
            OnPointsChanged();
        }

        private void plusTenButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                double x = rng.NextDouble() * 500, y = rng.NextDouble() * 500;
                points.Add(new PointF((float)x, (float)y));
            }

            OnPointsChanged();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            points.Clear();
            rng = InitRNG();
            OnPointsChanged();
        }
    }
}
