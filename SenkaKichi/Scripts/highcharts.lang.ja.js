$(function () {
    Highcharts.setOptions({
        lang: {
            loading: '読み込み中...',
            months: ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月'],
            shortMonths: ['1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'],
            weekdays: ['日曜日', '月曜日', '火曜日', '水曜日', '木曜日', '金曜日', '土曜日'],
            shortWeekdays: ['日', '月', '火', '水', '木', '金', '土'],
            decimalPoint: '.',
            numericSymbols: ['k', 'M', 'G', 'T', 'P', 'E'], // SI prefixes used in axis labels
            resetZoom: 'ズームをリセット',
            resetZoomTitle: '1:1にズームレベルをリセット',
            thousandsSep: '',
            downloadPng: 'PNG画像をダウンロード (透明)',
            downloadPngFill: 'PNG画像をダウンロード　(白)',
            downloadSvg: 'SVGベクタ画像をダウンロード',
            contextButtonTitle: 'ダウンロード'
        },
        xAxis: {
            dateTimeLabelFormats: {
                day: '%b%e日(%a)',
                week: '%b%e日',
                month: '%y年%b'
            }
        },
        tooltip: {
            dateTimeLabelFormats: {
                millisecond: '%A, %b%e日, %H:%M:%S.%L',
                second: '%b%e日 %H:%M:%S',
                minute: '%b%e日 %H:%M',
                hour: '%b%e日 %H:%M',
                day: '%b%e日 %A',
                week: '%Y年%b%e日',
                month: '%Y年%b',
                year: '%Y年'
            }
        }
    });
});