$(function () {
    var series = [
        {
            index: 2,
            name: SenkaChart.PlayerName,
            data: SenkaChart.RankPoint.Value,
            color: '#958242'
        },
        {
            index: 1,
            name: SenkaChart.RankPointLower.Key + '位',
            data: SenkaChart.RankPointLower.Value,
            color: '#101010',
            marker: { symbol: 'diamond' }
        }
    ];
    if (SenkaChart.RankPointUpper.Key != 0) {
        series.push({
            index: 0,
            name: SenkaChart.RankPointUpper.Key + '位',
            data: SenkaChart.RankPointUpper.Value,
            color: '#909090',
            marker: { symbol: 'diamond' }
        });
    }

    $('#RankPointChart').highcharts({
        chart: {
            type: 'area',
            zoomType: 'x'
        },
        title: {
            text: '戦果履歴'
        },
        subtitle: {
            text: SenkaChart.PlayerName + ' (' + SenkaChart.ServerName + ')'
        },
        credits: {
            text: SenkaChart.Date + ' - 戦果基地',
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
        legend: {
            labelFormatter: function () {
                return this.symbol == 'circle' ? this.name + '　(' + SenkaChart.RankPoint.Key + '位)' : this.name;
            }
        },
        tooltip: {
            shared: true,
            crosshairs: true,
            formatter: function (tooltip) {
                var items = this.points;
                // sort the values
                items.sort(function (a, b) {
                    return ((a.y < b.y) ? 1 : ((a.y > b.y) ? -1 : 0));
                });

                return tooltip.defaultFormatter.call(this, tooltip);
            }
        },
        series: series
    });
});