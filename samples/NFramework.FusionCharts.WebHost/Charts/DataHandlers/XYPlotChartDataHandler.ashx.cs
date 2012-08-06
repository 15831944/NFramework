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
    public class XYPlotChartDataHandler : NSoft.NFramework.FusionCharts.Web.DataXmlHandlerBase {
        private static readonly Random rnd = new ThreadSafeRandom();

        #region Overrides of FusionChartDataXmlHandlerBase

        /// <summary>
        /// ���ϴ� Chart�� �����մϴ�.
        /// </summary>
        public override IChart BuildFusionChart() {
            var factoryId = Request["FactoryId"].AsInt(1);
            var chartName = Request["ChartName"].AsText("Scatter");

            var chart = new MultiSeriesChart
                        {
                            Caption = "Factory ����",
                            SubCaption = "���� ���귮",
                            Palette = rnd.Next(1, 5),
                            RotateLabels = true,
                            // PlaceValuesInside = true,
                            // RotateValues = true,
                            XAxisName = "Day",
                            YAxisName = "Units",
                            BaseFontAttr = { Font = "���� ���" },
                            BorderAttr = { Show = true },
                            BackgroundAttr = { BgColor = Color.White, BgAlpha = 100 },
                            ShowShadow = true
                        };

            var outputs = FactoryRepository.FindAllFactoryOutputByFactoryId(factoryId);
            foreach(var output in outputs) {
                chart.AddCategory(output.DatePro.Value.ToMonthDayString(), true);
                //var category = new CategoryElement
                //               {
                //                   Label = output.DatePro.Value.ToMonthDayString()
                //               };
                //chart.Categories.CategoryElements.Add(category);
            }


            for(int i = 1; i < 4; i++) {
                var dataSet = new DataSetElement
                              {
                                  SeriesName = "Factory " + i.ToString(),
                                  ShowValues = false
                              };
                FillData(dataSet, i, chartName);
                chart.DataSets.Add(dataSet);
            }

            return chart;
        }

        private static void FillData(DataSetElement dataSet, int factoryId, string chartName) {
            //
            // Build Data
            //
            var outputs = FactoryRepository.FindAllFactoryOutputByFactoryId(factoryId);

            var num = 0;
            foreach(var output in outputs) {
                SetElementBase set;
                if(chartName == "Scatter")
                    set = new XYSetElement(output.DatePro.Value.DayOfYear, output.Quantity ?? 0);
                else
                    set = new XYSetElement(output.DatePro.Value.DayOfYear, output.DatePro.Value.DayOfYear, output.Quantity ?? 0);

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