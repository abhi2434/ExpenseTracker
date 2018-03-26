//custom binding for showing popup modal
ko.bindingHandlers.myModal = {
   
    init: function (element, valueAccessor) {
        $(element).modal({
            show: false
        });

        var value = valueAccessor();
        if (typeof value === 'function') {
            $(element).on('hide.bs.modal', function () {
                value(false);
            });
        }
    },
    update: function (element, valueAccessor) {
        var value = valueAccessor();
        if (ko.utils.unwrapObservable(value)) {
            $(element).modal('show');
        } else {
            $(element).modal('hide');
        }
    }
}

// Custom Binding for slidedown and slideup
ko.bindingHandlers.slideVisible = {
    update: function (element, valueAccessor) {
        // First get the latest data that we're bound to
        var value = valueAccessor();

        // Next, whether or not the supplied model property is observable, get its current value
        var valueUnwrapped = ko.unwrap(value);

        // Now manipulate the DOM element
        if (valueUnwrapped == "opened")
            $(element).slideDown(); // Make the element visible
        else
            $(element).slideUp();   // Make the element invisible
    }
};

var prev;
//custom binding for making the list draggable
ko.bindingHandlers.uiSortableList = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        //alert(element);
        var list = valueAccessor();
        $(element).sortable({
            connectWith: '.list_group',
            cursor: "move",
            update: function (event, ui) {
                //retrieve our actual data item
                var item = ko.dataFor(ui.item[0]);
                //var x = ko.unwrap(item);

                //figure out its new position
                // var position = ko.utils.arrayIndexOf(ui.item.parent().children(), ui.item[0]);
                var bind = bindingContext.$data;
                //console.log(bindingContext.$data.values());
                if (item.TodoGroupId() == bindingContext.$data.GID()) {
                    prev = bind;
                }
                else {
                    item.TodoGroupId(bind.GID());
                    bind.Update(item, prev);
                    prev.items(prev.items()-1);
                    bind.items(bind.items()+1);
                }
            },
        });
    }
};

// Custom Binding for datetimepicker
(function (window, $, undefined) {
    if (window.ko && $.timepicker) {
        ko.bindingHandlers.datetimepicker = {
            init: function (element, valueAccessor, allBindingsAccessor) {
                //initialize datetimepicker with some optional options
                var options = allBindingsAccessor().datetimepickerOptions || {};
                $(element).datetimepicker(options);

                //handle the field changing
                ko.utils.registerEventHandler(element, "click", function () {
                        var observable = valueAccessor();
                        observable($(element).datetimepicker({ dateFormat: 'dd-mm-yy', timeFormat: 'HH:mm' }).val());
                    //    console.log($(element).datetimepicker({ dateFormat: 'dd-mm-yy', timeFormat: 'HH:mm'}).val());
                });

                //handle disposal (if KO removes by the template binding)
                ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                    $(element).datetimepicker("destroy");
                });

            },
            update: function (element, valueAccessor) {
                var value = ko.utils.unwrapObservable(valueAccessor());

                //handle date data coming via json from Microsoft
                if (String(value).indexOf('/Date(') == 0) {
                    value = new Date(parseInt(value.replace(/\/Date\((.*?)\)\//gi, "$1")));
                }

                current = $(element).datetimepicker({ dateFormat: 'dd-mm-yy', timeFormat: 'HH:mm' });

                if (value - current !== 0) {
                    $(element).datetimepicker("setDate", value);
                }
            }
        };
    }
}(window, jQuery));
