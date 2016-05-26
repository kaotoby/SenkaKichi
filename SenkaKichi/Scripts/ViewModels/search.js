$(function () {
    var engine = new Bloodhound({
        identify: function (o) { return o.Id; },
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('Id'),
        dupDetector: function (a, b) { return a.Id === b.Id; },
        remote: {
            url: '/player/suggest?q=',
            replace: function (url, query) {
                return url + query + '&server=' + $('#search-form select').val();
            },
            transform: function(response) {
                return response.success ? response.data : [];
            }
        },
        sufficient: 7
    });

    var template = function (o) {
        return '<a href="/player/' + o.Id + '">' +
            '<span class="badge">' + o.Server +'</span>' + 
            '<div><b>' + o.Name + '</b>&nbsp;&nbsp;' +
            '<small class="cl-gray">' + (o.Comment == '' ? '&nbsp;' : o.Comment) + '</small></div>' +
            '</a>';
    };

    var oldValue = 0, newValue = 0;
    $('#search-form select').change(function () {
        newValue = this.value;
    });

    $('#search-form .typeahead').typeahead({
        minLength: 1,
        checkValidUpdate: function (query, oldQuery) {
            var selectChanged = oldValue !== newValue;
            oldValue = newValue;
            return query !== oldQuery || selectChanged;
        },
        classNames: {
            cursor: 'active',
            dataset: 'list-group',
            suggestion: 'list-group-item'
        }
    }, {
        source: engine,
        displayKey: 'Name',
        limit: 7,
        templates: {
            suggestion: template
        }
    });
});