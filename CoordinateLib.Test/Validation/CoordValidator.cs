using CoordinateLib;
using CoordinateLib.CoordSystems;
using System;
using System.Collections.Generic;

namespace Tests.Validation
{
    public class CoordValidator
    {
        public class Result
        {
            private double max;
            private double sd;
            private double avg;
            private int count;

            public Result(double max, double avg, double sd, int count)
            {
                this.max = max;
                this.avg = avg;
                this.sd = sd;
                this.count = count;
            }

            public double Max
            {
                get { return this.max; }
            }

            public double Avg
            {
                get { return this.avg; }
            }

            public double SD
            {
                get { return this.sd; }
            }

            public int Count
            {
                get { return this.count; }
            }
        }

        private GeotransInputFile stimuli;
        private GeotransOutputFile expected;
        private Converter transform;

        public CoordValidator(GeotransInputFile stimuli, GeotransOutputFile expected, Converter transform)
        {
            this.stimuli = stimuli;
            this.expected = expected;
            this.transform = transform;
        }

        public Result Validate(CoordSystem coordSystem)
        {
            if (this.expected.Positions.Count != this.stimuli.Positions.Count)
            {
                throw new Exception("Expected and Stimuli don't match!");
            }

            double[] distance = new double[this.stimuli.Positions.Count];
            double sum = 0;
            double max = 0;
            int count = 0;
            for (int i = 0; i < this.stimuli.Positions.Count; i++)
            {
                if (this.expected.Positions[i].Valid)
                {
                    var result = transform.ConvertNumeric(stimuli.Positions[i].Latitude, stimuli.Positions[i].Longitude, (int)stimuli.Positions[i].Altitude, coordSystem);

                    double distance_e = expected.Positions[i].Easting - result.Easting;
                    double distance_n = expected.Positions[i].Northing - result.Northing;

                    distance[i] = Math.Sqrt(distance_e * distance_e + distance_n * distance_n);
                    sum += distance[i];
                    if (distance[i] > 4)
                    {
                        //System.Diagnostics.Debugger.Break();
                    }
                    max = Math.Max(max, distance[i]);
                    count++;
                }
                else
                {
                    distance[i] = double.NaN;
                }
            }

            double E = sum / (double)count;
            double sum2 = 0;
            for (int i = 0; i < stimuli.Positions.Count; i++)
            {
                if (!double.IsNaN(distance[i]))
                {
                    sum2 += (distance[i] - E) * (distance[i] - E);
                }
            }
            double std = Math.Sqrt(sum2 / (double)count);

            return new Result(max, E, std, count);
        }
    }
}
