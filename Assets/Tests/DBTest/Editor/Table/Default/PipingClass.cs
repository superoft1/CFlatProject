using System.Linq;
using Chiyoda.DB;
using NUnit.Framework;

namespace Tests.EditorMode.Editor.DB
{
  public class PipingClass
  {
    [SetUp]
    public void Init()
    {
      Setup.DB();
    }

    [Test]
    public void GetNameList()
    {
      var table = Chiyoda.DB.DB.Get<PipingClassTable>();
      Assert.NotNull(table);

      { // 全部
        var typeList = table.GetNameList();
        Assert.NotNull(typeList);
        Assert.NotZero(typeList.Count);

        Assert.IsTrue(typeList.Contains("CL150"));
        Assert.IsTrue(typeList.Contains("CL300"));
        Assert.IsTrue(typeList.Contains("CL600"));
        Assert.IsTrue(typeList.Contains("CL900"));
        Assert.IsTrue(typeList.Contains("CL1500"));
        Assert.IsTrue(typeList.Contains("CL2500"));
        Assert.IsTrue(typeList.Contains("JIS_Default"));
      }

      { // JISのみ
        var typeList = table.GetNameList("JIS");
        Assert.NotNull(typeList);
        Assert.NotZero(typeList.Count);

        Assert.IsTrue(typeList.Contains("JIS_Default"));
      }
    }

    [Test]
    public void GetPipeList()
    {
      var table = Chiyoda.DB.DB.Get<PipingClassTable>();
      Assert.NotNull(table);

      // 正常系 CL150
      {
        var pipes = table.GetPipes("CL150");
        Assert.NotNull(pipes);
        Assert.NotZero(pipes.Count);

        {
          var pipe = pipes.Where(rec => rec.NPSmm == 6);
          Assert.IsTrue(pipe.Any());
          Assert.AreEqual(6, pipe.First().NPSmm);
          Assert.AreEqual("BE", pipe.First().EndPrep);
          Assert.AreEqual("STD", pipe.First().IdentificationNote);
          Assert.AreEqual(40, pipe.First().Schedule.Value);

          Assert.AreEqual("PIP", pipe.First().ShortCode);
          Assert.AreEqual("ASME", pipe.First().Standard);
          Assert.AreEqual("PIP_ASME_6_STD_BE", pipe.First().IdentCode);
        }
        {
          var pipe = pipes.Where(rec => rec.NPSmm == 600);
          Assert.IsTrue(pipe.Any());
          Assert.AreEqual(600, pipe.First().NPSmm);
          Assert.AreEqual("BE", pipe.First().EndPrep);
          Assert.AreEqual("STD", pipe.First().IdentificationNote);
          Assert.AreEqual(20, pipe.First().Schedule.Value);

          Assert.AreEqual("PIP", pipe.First().ShortCode);
          Assert.AreEqual("ASME", pipe.First().Standard);
          Assert.AreEqual("PIP_ASME_600_STD_BE", pipe.First().IdentCode);
        }
      }

      // TODO 正常系 CL2500
      {

      }

      // TODO 正常系 JIS
      { 


      }

      // 異常系 -> 要素数ゼロのリストを返す
      // TODO RecordNotFoundExceptionにする
      {
        var pipes = table.GetPipes("XXX");
        Assert.NotNull(pipes);
        Assert.Zero(pipes.Count);
      }
    }

    // TODO GetFlangeList
    [Test]
    public void GetFlangeList()
    {
    }

    // TODO GetElbowList
    [Test]
    public void GetElbowList()
    {
      var table = Chiyoda.DB.DB.Get<PipingClassTable>();
      Assert.NotNull(table);

      // 正常系 -> CL150
      {
        var elbows = table.GetElbows("CL150");
        Assert.NotNull(elbows);
        Assert.NotZero(elbows.Count);

        // 90deg 最小(6mm)
        {
          var elbow = elbows.Where(rec => rec.NPSmm == 6 && rec.ShortCode == "90E");
          Assert.IsTrue(elbow.Any());
          Assert.AreEqual(6, elbow.First().NPSmm);
          Assert.AreEqual("BW", elbow.First().EndPrep);
          Assert.AreEqual("STD", elbow.First().IdentificationNote);
          Assert.AreEqual(40, elbow.First().Schedule.Value);

          Assert.AreEqual("ASME", elbow.First().Standard);
          Assert.AreEqual("90E_ASME_6_STD_BW", elbow.First().IdentCode);
        }

        // 90deg 最大(600mm)
        {
          var elbow = elbows.Where(rec => rec.NPSmm == 600 && rec.ShortCode == "90E");
          Assert.IsTrue(elbow.Any());
          Assert.AreEqual(600, elbow.First().NPSmm);
          Assert.AreEqual("BW", elbow.First().EndPrep);
          Assert.AreEqual("STD", elbow.First().IdentificationNote);
          Assert.AreEqual(20, elbow.First().Schedule.Value);

          Assert.AreEqual("ASME", elbow.First().Standard);
          Assert.AreEqual("90E_ASME_600_STD_BW", elbow.First().IdentCode);
        }

        // 45deg 最小(6mm)
        {
          var elbow = elbows.Where(rec => rec.NPSmm == 6 && rec.ShortCode == "45E");
          Assert.IsTrue(elbow.Any());
          Assert.AreEqual(6, elbow.First().NPSmm);
          Assert.AreEqual("BW", elbow.First().EndPrep);
          Assert.AreEqual("STD", elbow.First().IdentificationNote);
          Assert.AreEqual(40, elbow.First().Schedule.Value);

          Assert.AreEqual("ASME", elbow.First().Standard);
          Assert.AreEqual("45E_ASME_6_STD_BW", elbow.First().IdentCode);
        }

        // 45deg 最大(600mm)
        {
          var elbow = elbows.Where(rec => rec.NPSmm == 600 && rec.ShortCode == "45E");
          Assert.IsTrue(elbow.Any());
          Assert.AreEqual(600, elbow.First().NPSmm);
          Assert.AreEqual("BW", elbow.First().EndPrep);
          Assert.AreEqual("STD", elbow.First().IdentificationNote);
          Assert.AreEqual(20, elbow.First().Schedule.Value);

          Assert.AreEqual("ASME", elbow.First().Standard);
          Assert.AreEqual("45E_ASME_600_STD_BW", elbow.First().IdentCode);
        }

      }

      // 異常系 -> 要素数ゼロのリストを返す
      {
        Assert.Throws<NoRecordFoundException>(() => table.GetElbows("XXX"));
      }


    }

    // TODO GetTeeList
    [Test]
    public void GetTeeList()
    {
      var table = Chiyoda.DB.DB.Get<PipingClassTable>();
      Assert.NotNull(table);

      // 正常系 -> CL150
      {
        var tees = table.GetTees("CL150");
        Assert.NotNull(tees);
        Assert.NotZero(tees.Count);

        // 最小 NPS_H(PipeThickness) 15
        {
          var min_npsh = tees.Where(rec => rec.NPS_H_mm == 15).ToList();
          
          //NPS_B 最小
          {
            var min_npsb = min_npsh.Where(rec => rec.NPS_B_mm == min_npsh.Min(min=> min.NPS_B_mm)).ToList();
            Assert.AreEqual(1, min_npsb.Count);
            Assert.AreEqual(40, min_npsb.First().Schedule.Value);
            Assert.AreEqual("TEE_ASME_PT15/NPSB8_STD_BW", min_npsb.First().IdentCode);
          }

          //NPS_B 最大
          {
            var max_npsb = min_npsh.Where(rec => rec.NPS_B_mm == min_npsh.Max(max => max.NPS_B_mm)).ToList();
            Assert.AreEqual(1, max_npsb.Count);
            Assert.AreEqual(40, max_npsb.First().Schedule.Value);
            Assert.AreEqual("TEE_ASME_PT15/NPSB15_STD_BW", max_npsb.First().IdentCode);
          }
        }

        // 最大 NPS_H(PipeThickness) 600
        {
          var max_npsh = tees.Where(rec => rec.NPS_H_mm == 600).ToList();

          //NPS_B 最小
          {
            var min_npsb = max_npsh.Where(rec => rec.NPS_B_mm == max_npsh.Min(min => min.NPS_B_mm)).ToList();
            Assert.AreEqual(1, min_npsb.Count);
            Assert.AreEqual(20, min_npsb.First().Schedule.Value);
            Assert.AreEqual("TEE_ASME_PT600/NPSB250_STD_BW", min_npsb.First().IdentCode);
          }

          //NPS_B 最大
          {
            var max_npsb = max_npsh.Where(rec => rec.NPS_B_mm == max_npsh.Max(max => max.NPS_B_mm)).ToList();
            Assert.AreEqual(1, max_npsb.Count);
            Assert.AreEqual(20, max_npsb.First().Schedule.Value);
            Assert.AreEqual("TEE_ASME_PT600/NPSB600_STD_BW", max_npsb.First().IdentCode);
          }
        }

      }

      // 異常系 -> 要素数ゼロのリストを返す
      {
        Assert.Throws<NoRecordFoundException>(() => table.GetTees("XXX"));
      }

    }

    // TODO GetReducerList
    [Test]
    public void GetReducerList()
    {
      var table = Chiyoda.DB.DB.Get<PipingClassTable>();
      Assert.NotNull(table);

      // 正常系 -> CL150
      {
        var reducers = table.GetReducers("CL150");
        Assert.NotNull(reducers);
        Assert.NotZero(reducers.Count);

        //CRE
        {
          var cre = reducers.Where(rec => rec.ShortCode == "CRE").ToList();

          // 最小 NPS_L(PipeThickness) 20
          {
            var min_npsl = cre.Where(rec => rec.NPS_L_mm == 20).ToList();
            //NPS_S 最小
            {
              var min_npsb = min_npsl.Where(rec => rec.NPS_S_mm == min_npsl.Min(min => min.NPS_S_mm)).ToList();
              Assert.AreEqual(1, min_npsb.Count);
              Assert.AreEqual(40, min_npsb.First().Schedule.Value);
              Assert.AreEqual("CRE_ASME_PT20/NPSS10_STD_BW", min_npsb.First().IdentCode);
            }

            //NPS_S 最大
            {
              var max_npsb = min_npsl.Where(rec => rec.NPS_S_mm == min_npsl.Max(max => max.NPS_S_mm)).ToList();
              Assert.AreEqual(1, max_npsb.Count);
              Assert.AreEqual(40, max_npsb.First().Schedule.Value);
              Assert.AreEqual("CRE_ASME_PT20/NPSS15_STD_BW", max_npsb.First().IdentCode);
            }
          }

          // 最大 NPS_L(PipeThickness) 600
          {
            var min_npsl = cre.Where(rec => rec.NPS_L_mm == 600).ToList();
            //NPS_S 最小
            {
              var min_npsb = min_npsl.Where(rec => rec.NPS_S_mm == min_npsl.Min(min => min.NPS_S_mm)).ToList();
              Assert.AreEqual(1, min_npsb.Count);
              Assert.AreEqual(20, min_npsb.First().Schedule.Value);
              Assert.AreEqual("CRE_ASME_PT600/NPSS400_STD_BW", min_npsb.First().IdentCode);
            }

            //NPS_S 最大
            {
              var max_npsb = min_npsl.Where(rec => rec.NPS_S_mm == min_npsl.Max(max => max.NPS_S_mm)).ToList();
              Assert.AreEqual(1, max_npsb.Count);
              Assert.AreEqual(20, max_npsb.First().Schedule.Value);
              Assert.AreEqual("CRE_ASME_PT600/NPSS550_STD_BW", max_npsb.First().IdentCode);
            }
          }
        }

        //ERE
        {
          var cre = reducers.Where(rec => rec.ShortCode == "ERE").ToList();

          // 最小 NPS_L(PipeThickness) 20
          {
            var min_npsl = cre.Where(rec => rec.NPS_L_mm == 20).ToList();
            //NPS_S 最小
            {
              var min_npsb = min_npsl.Where(rec => rec.NPS_S_mm == min_npsl.Min(min => min.NPS_S_mm)).ToList();
              Assert.AreEqual(1, min_npsb.Count);
              Assert.AreEqual(40, min_npsb.First().Schedule.Value);
              Assert.AreEqual("ERE_ASME_PT20/NPSS10_STD_BW", min_npsb.First().IdentCode);
            }

            //NPS_S 最大
            {
              var max_npsb = min_npsl.Where(rec => rec.NPS_S_mm == min_npsl.Max(max => max.NPS_S_mm)).ToList();
              Assert.AreEqual(1, max_npsb.Count);
              Assert.AreEqual(40, max_npsb.First().Schedule.Value);
              Assert.AreEqual("ERE_ASME_PT20/NPSS15_STD_BW", max_npsb.First().IdentCode);
            }
          }

          // 最大 NPS_L(PipeThickness) 600
          {
            var min_npsl = cre.Where(rec => rec.NPS_L_mm == 600).ToList();
            //NPS_S 最小
            {
              var min_npsb = min_npsl.Where(rec => rec.NPS_S_mm == min_npsl.Min(min => min.NPS_S_mm)).ToList();
              Assert.AreEqual(1, min_npsb.Count);
              Assert.AreEqual(20, min_npsb.First().Schedule.Value);
              Assert.AreEqual("ERE_ASME_PT600/NPSS400_STD_BW", min_npsb.First().IdentCode);
            }

            //NPS_S 最大
            {
              var max_npsb = min_npsl.Where(rec => rec.NPS_S_mm == min_npsl.Max(max => max.NPS_S_mm)).ToList();
              Assert.AreEqual(1, max_npsb.Count);
              Assert.AreEqual(20, max_npsb.First().Schedule.Value);
              Assert.AreEqual("ERE_ASME_PT600/NPSS550_STD_BW", max_npsb.First().IdentCode);
            }
          }

        }
      }
    }

  }
}