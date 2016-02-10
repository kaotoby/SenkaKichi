$(function () {
    var series = [];
    var colors = [ '#958242', '#808080', '#606060', '#404040', '#202020' ];
    for (var i = 0; i < SenkaChart.RankPoint.length; i++) {
        series.push({
            index: i,
            name: SenkaChart.RankPoint[i].Key + '位',
            data: SenkaChart.RankPoint[i].Value,
            color: colors[i],
            marker: { symbol: 'diamond' }
        });
    }
    series[0].marker = { symbol: 'circle' }

    $('#RankPointChart').highcharts({
        chart: {
            type: 'area',
            zoomType: 'x'
        },
        title: {
            text: '戦果履歴'
        },
        subtitle: {
            text: SenkaChart.ServerName
        },
        credits: {
            text: SenkaChart.Date + ' - 戦果基地',
        },
        exporting: {
            chartOptions: {
                credits: {
                    enabled: true
                }
            }
        },
        xAxis: {
            type: 'datetime',
            min: Date.parse(SenkaChart.StartTime) - 3 * 3600 * 1000,
            minRange: 48 * 3600 * 1000,
            tickPixelInterval: 125
        },
        yAxis: {
            title: {
                text: '戦果',
			    rotation: 0
            }
        },
        plotOptions: {
            area: {
                connectNulls: true
            },
            series: {
                pointStart: Date.parse(SenkaChart.StartTime),
                pointInterval: 12 * 3600 * 1000
            }
        },
        tooltip: {
            shared: true,
            crosshairs: true,
            formatter: function (tooltip) {
                var index = this.points[0].series.xData.indexOf(this.points[0].x);
                return tooltip.defaultFormatter.call(this, tooltip).replace('1位', SenkaChart.TopName[index]);
            }
        },
        series: series
    });
});