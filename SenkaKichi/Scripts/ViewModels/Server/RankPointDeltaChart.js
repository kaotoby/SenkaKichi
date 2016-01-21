$(function () {
    var xAxis = [];
    for (var i = 0; i < SenkaChart.RankPoint.length; i++) {
        xAxis.push(SenkaChart.RankPoint[i].Key + '位');
    }

    $('#RankPointDeltaChart').highcharts({
        chart: {
            type: 'bar'
        },
        title: {
            text: '戦果増分履歴'
        },
        subtitle: {
            text: SenkaChart.ServerName
        },
        credits: {
            text: SenkaChart.Date + ' - 戦果基地',
        },
        xAxis: {
            categories: xAxis
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
            enabled: false
        },
        plotOptions: {
            series: {
                stacking: 'normal',
                dataLabels: {
                    enabled: true,
                    color: 'white',
                    style: {
                        textShadow: '0 0 3px black'
                    }
                }
            }
        },
        series: [{
            name: '3時～15時',
            color: '#AA985A',
            data: SenkaChart.RankPointDeltaPm
        }, {
            name: '前日15時～3時',
            color: '#4A432A',
            data: SenkaChart.RankPointDeltaAm
        }]
    });
});