using System;
using System.Drawing;
using System.Web.Services;
using NSoft.NFramework.FusionCharts.Charts;
using NSoft.NFramework.FusionCharts.WebHost.Domain.Services;
using NSoft.NFramework.Threading;
using NSoft.NFramework.Tools;
using NSoft.NFramework.Web.Tools;

namespace NSoft.NFramework.FusionCharts.WebHost.Charts.DataHandlers {
    /// <summary>
    /// $codebehindclassname$�� ��� �����Դϴ�.
    /// </summary>
    [WebService(Namespace = "http://ws.realweb21.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class CombinationChartDataHandler : NSoft.NFramework.FusionCharts.Web.DataXmlHandlerBase {
        private static readonly Random rnd = new ThreadSafeRandom();

        #region Overrides of FusionChartDataXmlHandlerBase

        /// <summary>
        /// ���ϴ� Chart�� �����մϴ�.
        /// </summary>
        public override IChart BuildFusionChart() {
            var factoryId = Request["FactoryId"].AsInt(1);
            var lineDual = Request["LineDual"].AsInt(0);
            var numVisiblePlot = Request["numVisiblePlot"].AsInt(12);

            var chart = new MultiSeriesChart
                        {
                            Caption = "Factory ����",
                            SubCaption = "���� ���귮",
                            Palette = rnd.Next(1, 5),
                            RotateLabels = true,
                            // PlaceValuesInside = true,
                            // RotateValues = true,
                            SlantLabels = true,
                            XAxisName = "Day",
                            YAxisName = "Units",
                            NumVisiblePlot = numVisiblePlot,
                            BaseFontAttr = { Font = "���� ���" },
                            BorderAttr = { Show = true },
                            BackgroundAttr = { BgColor = Color.White, BgAlpha = 100 }
                        };

            chart.ShowShadow = true;

            var outputs = FactoryRepository.FindAllFactoryOutputByFactoryId(factoryId);
            foreach(var output in outputs) {
                chart.AddCategory(output.DatePro.Value.ToMonthDayString(), true);
                //var category = new CategoryElement
                //               {
                //                   Label = output.DatePro.Value.ToMonthDayString()
                //               };
                // chart.Categories.CategoryElements.Add(category);
            }


            for(int i = 1; i < 4; i++) {
                var dataSet = new DataSetElement
                              {
                                  SeriesName = "Factory " + i.ToString(),
                                  ShowValues = false,
                                  RenderAs = (i % 4 == 1) ? GraphKind.Area : ((i % 4 == 3) ? GraphKind.Line : GraphKind.Column)
                              };

                if(lineDual == 1 && i == 3) {
                    dataSet.RenderAs = GraphKind.Line;
                    dataSet.ParentYAxis = YAxisKind.S;
                }

                FillData(dataSet, i);
                chart.DataSets.Add(dataSet);
            }

            chart.TrendLines.Add(new DoubleLineElement
                                 {
                                     StartValue = 120,
                                     DisplayValue = "Good",
                                     Color = "#009933".FromHtml()
                                 });
            //chart.TrendLines.TrendlineElements.Add(new DoubleLineElement
            //                                       {
            //                                           StartValue = 120,
            //                                           DisplayValue = "Good",
            //                                           Color = "#009933".FromHtml()
            //                                       });

            return chart;
        }

        private static void FillData(DataSetElement dataSet, int factoryId) {
            //
            // Build Data
            //
            var outputs = FactoryRepository.FindAllFactoryOutputByFactoryId(factoryId);

            var num = 0;
            foreach(var output in outputs) {
                var set = new ValueSetElement
                          {
                              //Label = output.DatePro.Value.ToShortDateString(),
                              Value = output.Quantity ?? 0
                          };

                if((num++ % 2) == 0)
                    set.Link.SetLink(FusionLinkMethod.Local, "javascript:PopUp('April');");
                        // ���� Javascript �޼ҵ带 ������ "PopUp-April" �θ� ���� �ȴ�.
                else {
                    set.Link.SetLink(FusionLinkMethod.PopUp,
                                     WebTool.GetScriptPath("~/Charts/Ajax/Default.aspx?FactoryId=") + factoryId.ToString());
                    set.Link.Width = 600;
                    set.Link.Height = 400;
                }

                dataSet.AddSet(set);
            }
        }

        #endregion
    }
}