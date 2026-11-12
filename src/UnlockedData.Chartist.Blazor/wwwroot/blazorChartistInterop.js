class BlazorChartist {

    constructor() {
        const loadError = this.chartistLoadError();
        if (loadError) {
            console.error(loadError);
        }
    }

    // Returns the problem with the script tags on the page, or null when Chartist is ready to use.
    chartistLoadError() {
        const chartist = globalThis.Chartist;
        if (chartist && chartist.LineChart && chartist.compat && chartist.plugins && chartist.plugins.legend) {
            return null;
        }

        return 'UnlockedData.Chartist.Blazor: Chartist is not loaded correctly. Load these scripts in this order: '
            + 'vendor/chartist/index.umd.js, vendor/chartist/compat.umd.js, chartist-plugins.js, blazorChartistInterop.js.';
    }

    assertChartistLoaded() {
        const loadError = this.chartistLoadError();
        if (loadError) {
            throw new Error(loadError);
        }
    }

    createChart(type, elem, data, options, instance, id) {

        this.assertChartistLoaded();

        //configure plugins if they are set
        options = this.optionsCleaner(type, options);
        options = this.configurePlugins(options);



        let chart;

        switch (type) {
            case "Bar":
                chart = new Chartist.BarChart(elem, data, options);
                break;
            case "Pie":
                chart = new Chartist.PieChart(elem, data, options);
                break;
            case "Line":
                chart = new Chartist.LineChart(elem, data, options);
                break;
        }

        chart.on('created', function (data) {

            switch (type) {
                case "Bar":
                    //each bar fires off
                    elem.querySelectorAll('.' + options.classNames.bar).forEach(function (item) {
                        item.addEventListener('click', function (e) {
                            e = bChartist.createChartistMouseEvents(e);
                            instance.invokeMethodAsync("JSDomDataPointClicked", e);
                        });
                        item.addEventListener('mouseenter', function (e) {
                            e = bChartist.createChartistMouseEvents(e);
                            instance.invokeMethodAsync("JSDomDataPointEntered", e);
                        });
                        item.addEventListener('mouseleave', function (e) {
                            e = bChartist.createChartistMouseEvents(e);
                            instance.invokeMethodAsync("JSDomDataPointExited", e);
                        });
                    });

                    break;
                case "Pie":
                    break;
                case "Line":
                    //each point fires off
                    elem.querySelectorAll('.' + options.classNames.point).forEach(function (item) {
                        item.addEventListener('click', function (e) {
                            e = bChartist.createChartistMouseEvents(e);
                            instance.invokeMethodAsync("JSDomDataPointClicked", e);
                        });
                        item.addEventListener('mouseenter', function (e) {
                            e = bChartist.createChartistMouseEvents(e);
                            instance.invokeMethodAsync("JSDomDataPointEntered", e);
                        });
                        item.addEventListener('mouseleave', function (e) {
                            e = bChartist.createChartistMouseEvents(e);
                            instance.invokeMethodAsync("JSDomDataPointExited", e);
                        });
                    });
                    break;
            }

            options.cssClassUpdates.forEach(c => {
                elem.querySelectorAll(c.cssSelector).forEach(x => x.classList.add(c.class));
            })
        });

        elem['_chart'] = chart;
    };

    updateChart(type, elem, data, options) {

        this.assertChartistLoaded();

        options = this.configurePlugins(options);
        options = this.optionsCleaner(type, options);
        elem['_chart'].update(data, options);


    };

    createChartistMouseEvents(e) {
        let values = e.target.getAttribute('ct:value').split(',');
        let result = {
            pointMetaInfo: this.pointMetaInfo(e.target),
            x: function () {
                if (values.length === 2) {
                    return Number(values[0]);
                }
            }(),
            y: function () {
                if (values.length === 2) {
                    return Number(values[1]);
                }
                return Number(values[0]);
            }(),
            seriesName: this.seriesName(e.target)
        };

        return result;
    };

    // A point without usable meta binds to the default PointMetaInfo on the C# side
    pointMetaInfo(element) {
        const meta = Chartist.deserialize(element.getAttribute('ct:meta'));
        if (meta === null || typeof meta !== 'object' || Array.isArray(meta)) {
            return {};
        }

        return meta;
    }

    // Chartist leaves ct:series-name off the series group of an unnamed series.
    // The attribute is namespaced, so a CSS attribute selector can't find it.
    seriesName(element) {
        for (let parent = element.parentElement; parent; parent = parent.parentElement) {
            const name = parent.getAttribute('ct:series-name');
            if (name !== null) {
                return name;
            }
        }

        return null;
    }

    interpolationHelper(method) {
        let result;
        switch (method) {
            case "monotone":
                result = Chartist.Interpolation.monotoneCubic;
                break;
            case "step":
                result = Chartist.Interpolation.step;
                break;
            case "none":
                result = Chartist.Interpolation.none;
                break;
            case "simple":
                result = Chartist.Interpolation.simple;
                break;
            case "cardinal":
                result = Chartist.Interpolation.cardinal;
                break;

        }

        return result;
    }

    optionsCleaner(type, options) {

        //configure axis options
        if (type == "Line" || type == "Bar") {

            options.axisX.labelInterpolationFnc = this.labelInterpolationFunctions[options.axisX.labelInterpolationFnc];
            options.axisY.labelInterpolationFnc = this.labelInterpolationFunctions[options.axisY.labelInterpolationFnc];

            options.axisX.type = Chartist[options.axisX.type];
            options.axisY.type = Chartist[options.axisY.type];
        }

        if (type == "Line") {
            options.lineSmooth = this.interpolationHelper(options.interpolationType)(options.interpolationOptions);

        }
        if (options.toolTipOptions.transformTooltipTextFunction != undefined) {
            options.toolTipOptions.transformTooltipTextFnc = this.labelInterpolationFunctions[options.toolTipOptions.transformTooltipTextFunction]
        }


        return options

    }

    configurePlugins(options) {
        // compat restores the 'created' event and runs the 1.x-style plugins below
        options.plugins = [Chartist.compat];

        if (options.showLegend) {
            options.plugins.push(Chartist.plugins.legend(options.legendOptions));
        }


        if (options.showTooltips) {
            // var transFunc = function (value) {console.log(value); return 'hi'};
            // var textFunc = function (meta, value) {console.log(value); return value.toString()};
            // options.toolTipOptions.tooltipFnc  = textFunc;
            // options.toolTipOptions.transformTooltipTextFnc  = transFunc;
            options.plugins.push(Chartist.plugins.tooltip(options.toolTipOptions));
        }

        if (options.showPointLabels) {
            options.plugins.push(Chartist.plugins.ctPointLabels());
        }

        return options
    }

    labelInterpolationFunctions = {
        truncateToKMB: function (value) {
            return Math.abs(Number(value)) >= 1.0e+9 ? Math.abs(Number(value)) / 1.0e+9 + "B" :
                Math.abs(Number(value)) >= 1.0e+6 ? Math.abs(Number(value)) / 1.0e+6 + "M" :
                    Math.abs(Number(value)) >= 1.0e+3 ? Math.abs(Number(value)) / 1.0e+3 + "K" : Math.abs(Number(value));
        },
        truncateToKMBCommaSeparated: function (value) {
            return Math.abs(Number(value)) >= 1.0e+9 ? Math.round(Number(value),0) / 1.0e+9 + "B" :
                Math.abs(Number(value)) >= 1.0e+6 ? Math.round(Number(value),0) / 1.0e+6 + "M" :
                    Math.abs(Number(value)) >= 1.0e+3 ? Math.round(Number(value),0)/ 1.0e+3 + "K" : Number(value);
        },
        commaSeparated: function (value) {
            return  Math.round(Number(value),0).toLocaleString()
        },
        noop: function (value) {
            return value;
        }
    }


    appendElemToSvg(parentElem, elem) {
        parentElem.append(elem);
    }
}

window.bChartist = new BlazorChartist();
