using CoordinateLib;
using CoordinateLib.CoordSystems;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Validation;

namespace Tests
{
    public class Tests
    {
        private List<SysValidation> systemValidations;
        private List<CoordSystem> coordSystems;
        private Converter converter;

        [SetUp]
        public void Setup()
        {
            systemValidations = new SysValidationsFile("Validation.xml").Systems;
            coordSystems = new CoordSystemsParser().Parse();
            converter = new Converter();
        }

        [Test]
        public void ValidateCoordinates()
        {
            var totalPass = true;
            foreach (var validation in systemValidations)
            {
                var coordSystem = coordSystems.SingleOrDefault(x => x.Name == validation.CoordSystemName);
                if (coordSystem == null)
                    Assert.Fail("coordSystem not found");
                else
                {
                    var inputData = new GeotransInputFile(validation.StimuliFile);
                    var outputData = new GeotransOutputFile(validation.ExpectedResponseFile);

                    var validationResult = new CoordValidator(inputData, outputData, converter).Validate(coordSystem);

                    Console.WriteLine("  --- {0} ---", coordSystem.Name);
                    Console.WriteLine("  INFO coordinate transformations={0}", validationResult.Count);
                    Console.WriteLine("  INFO max={0} avg={1} SD={2}", validationResult.Max, validationResult.Avg, validationResult.SD);

                    Console.WriteLine("  INFO tolerance_max={0} tolerance_avg={1}", validation.ToleranceMax, validation.ToleranceAvg);
                    bool pass = true;
                    if (validation.ToleranceAvg > 0 && validationResult.Avg > validation.ToleranceAvg)
                    {
                        Console.WriteLine("  INFO calculated avg ({0}) > tolerance ({1})", validationResult.Avg, validation.ToleranceAvg);
                        pass = false;
                    }

                    if (validation.ToleranceMax > 0 && validationResult.Max > validation.ToleranceMax)
                    {
                        Console.WriteLine("  INFO calculated max ({0}) > tolerance ({1})", validationResult.Max, validation.ToleranceMax);
                        pass = false;
                    }

                    if (pass)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("  PASSED");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("  FAILED");
                        Console.ResetColor();
                        totalPass = false;
                    }
                    Console.WriteLine();
                }
            }

            if (totalPass)
                Assert.Pass("Look at the output for more details.");
            else
                Assert.Fail("Some tests failed. Look at the output for more details.");
        }
    }
}