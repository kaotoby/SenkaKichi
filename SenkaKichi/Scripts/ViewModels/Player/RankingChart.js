$(function () {
    $('#RankingChart').highcharts({
        chart: {
            zoomType: 'x'
        },
        title: {
            text: '順位履歴'
        },
        subtitle: {
            text: SenkaChart.PlayerName + ' (' + SenkaChart.ServerName + ')'
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
            title: {
                text: '　'
            },
            type: 'datetime',
            min: Date.parse(SenkaChart.StartTime) - 3 * 3600 * 1000,
            minRange: 48 * 3600 * 1000,
            tickPixelInterval: 125
        },
        yAxis: {
            title: {
                text: '順位',
                rotation: 0
            },
            reversed: true,
            startOnTick: false,
            floor: 1,
            ceiling: 990,
            minTickInterval: 1
        },
        legend: {
            enabled: false
        },
        tooltip: {
            crosshairs: true,
            pointFormat: '<tr style="font-weight:bold"><td style="color:#958242">{series.name}</td><td>&nbsp;&nbsp;{point.y}位</td></tr>',
            style: {
                fontSize: '14px'
            }
        },
        series: [{
            pointInterval: 12 * 3600 * 1000,
            pointStart: Date.parse(SenkaChart.StartTime),
            name: SenkaChart.PlayerName,
            data: SenkaChart.Ranking,
            color: '#1F1E11',
            marker: {
                lineWidth: 2,
                lineColor: '#1F1E11',
                fillColor: '#B5A262'
            }
        }]
    });
});