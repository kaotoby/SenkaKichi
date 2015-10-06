$(function () {
    var series = [];
    if (SenkaChart.RankPointDeltaExtra != null) {
        series.push({
            name: 'ボーナス戦果',
            color: '#CC0000',
            data: SenkaChart.RankPointDeltaExtra
        });
    }
    series.push({
        name: '3時～15時',
        color: '#AA985A',
        data: SenkaChart.RankPointDeltaPm
    });
    series.push({
        name: '前日15時～3時',
        color: '#4A432A',
        data: SenkaChart.RankPointDeltaAm
    });

    $('#RankPointDeltaChart').highcharts({
        chart: {
            type: 'column',
            zoomType: 'x'
        },
        title: {
            text: '戦果増分履歴'
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
            tickPixelInterval: 130
        },
        yAxis: {
            title: {
                text: '戦果',
                rotation: 0
            },
            stackLabels: {
                enabled: true,
                style: {
                    fontWeight: 'bold',
                    color: 'gray'
                }
            },
            min: 0
        },
        legend: {
            reversed: true
        },
        tooltip: {
            shared: true,
            useHTML: true,
            pointFormat: '<tr><td>{series.name}</td><td><b>&nbsp;&nbsp;{point.y}</b></td></tr>',
            footerFormat: '',
            formatter: function (tooltip) {
                this.points.reverse();
                return tooltip.defaultFormatter.call(this, tooltip) +
                    '<tr style="color:#958242;border-top:1px solid #777"><td>合計</td><td><b>&nbsp;&nbsp;' + this.points[0].total + '</b></td></tr></table>';
            }
        },
        plotOptions: {
            column: {
                stacking: 'normal',
                dataLabels: {
                    enabled: true,
                    padding: 3,
                    color: 'white',
                    style: {
                        textShadow: '0 0 3px black'
                    }
                }
            },
            series: {
                pointStart: Date.parse(SenkaChart.StartTime) - 3 * 3600 * 1000,
                pointInterval: 24 * 3600 * 1000
            }
        },
        series: series
    });
});