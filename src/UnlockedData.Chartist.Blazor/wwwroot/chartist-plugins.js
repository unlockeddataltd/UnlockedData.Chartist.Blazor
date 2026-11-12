// First-party chart plugins for UnlockedData.Chartist.Blazor: legend, tooltip and point labels.
// They are 1.x-style plugins and run through Chartist's compat build, so this file
// must load after vendor/chartist/index.umd.js and vendor/chartist/compat.umd.js.
if (!globalThis.Chartist || !globalThis.Chartist.LineChart || !globalThis.Chartist.compat || !globalThis.Chartist.plugins) {
    throw new Error('UnlockedData.Chartist.Blazor: chartist-plugins.js must load after vendor/chartist/index.umd.js and vendor/chartist/compat.umd.js.');
}


// Internet Systems Consortium license
// ===================================
//
// Copyright (c) 2016, Code Yellow B.V.
//
//     Permission to use, copy, modify, and/or distribute this software for any purpose
// with or without fee is hereby granted, provided that the above copyright notice
// and this permission notice appear in all copies.
// https://github.com/CodeYellowBV/chartist-plugin-legend

(function (root, factory) {
        if (typeof define === 'function' && define.amd) {
            // AMD. Register as an anonymous module.
            define(['chartist'], function (chartist) {
                return (root.returnExportsGlobal = factory(chartist));
            });
        } else if (typeof exports === 'object') {
            // Node. Does not work with strict CommonJS, but
            // only CommonJS-like enviroments that support module.exports,
            // like Node.
            module.exports = factory(require('chartist'));
        } else {
            root['Chartist.plugins.legend'] = factory(root.Chartist);
        }
    }
    (this, function (Chartist) {
            /**
             * This Chartist plugin creates a legend to show next to the chart.
             *
             */
            'use strict';

            var defaultOptions = {
                className: '',
                classNames: false,
                removeAll: false,
                legendNames: false,
                clickable: true,
                onClick: null,
                position: 'top'
            };

            Chartist.plugins = Chartist.plugins || {};

            Chartist.plugins.legend = function (options) {

                // Catch invalid options
                if (options && options.position) {
                    if (!(options.position === 'top' || options.position === 'bottom' || options.position instanceof HTMLElement)) {
                        throw Error('The position you entered is not a valid position');
                    }
                    if (options.position instanceof HTMLElement) {
                        // Detatch DOM element from options object, because Chartist.extend
                        // currently chokes on circular references present in HTMLElements
                        var cachedDOMPosition = options.position;
                        delete options.position;
                    }
                }

                options = Chartist.extend({}, defaultOptions, options);

                if (cachedDOMPosition) {
                    // Reattatch the DOM Element position if it was removed before
                    options.position = cachedDOMPosition
                }

                return function legend(chart) {

                    function removeLegendElement() {
                        var legendElement = chart.container.querySelector('.ct-legend');
                        if (legendElement) {
                            legendElement.parentNode.removeChild(legendElement);
                        }
                    }

                    // Set a unique className for each series so that when a series is removed,
                    // the other series still have the same color.
                    function setSeriesClassNames() {
                        chart.data.series = chart.data.series.map(function (series, seriesIndex) {
                            if (typeof series !== 'object') {
                                series = {
                                    value: series
                                };
                            }
                            series.className = series.className || chart.options.classNames.series + '-' + Chartist.alphaNumerate(seriesIndex);
                            return series;
                        });
                    }

                    function createLegendElement() {
                        var legendElement = document.createElement('ul');
                        legendElement.className = 'ct-legend';
                        if (chart instanceof Chartist.Pie) {
                            legendElement.classList.add('ct-legend-inside');
                        }
                        if (typeof options.className === 'string' && options.className.length > 0) {
                            legendElement.classList.add(options.className);
                        }
                        if (chart.options.width) {
                            legendElement.style.cssText = 'width: ' + chart.options.width + 'px;margin: 0 auto;';
                        }
                        return legendElement;
                    }

                    // Get the right array to use for generating the legend.
                    function getLegendNames(useLabels) {
                        return options.legendNames || (useLabels ? chart.data.labels : chart.data.series);
                    }

                    // Initialize the array that associates series with legends.
                    // -1 indicates that there is no legend associated with it.
                    function initSeriesMetadata(useLabels) {
                        var seriesMetadata = new Array(chart.data.series.length);
                        for (var i = 0; i < chart.data.series.length; i++) {
                            seriesMetadata[i] = {
                                data: chart.data.series[i],
                                label: useLabels ? chart.data.labels[i] : null,
                                legend: -1
                            };
                        }
                        return seriesMetadata;
                    }

                    function createNameElement(i, legendText, classNamesViable) {
                        var li = document.createElement('li');
                        li.classList.add('ct-series-' + i);
                        // Append specific class to a legend element, if viable classes are given
                        if (classNamesViable) {
                            li.classList.add(options.classNames[i]);
                        }
                        li.setAttribute('data-legend', i);
                        li.textContent = legendText;
                        return li;
                    }

                    // Append the legend element to the DOM
                    function appendLegendToDOM(legendElement) {
                        if (!(options.position instanceof HTMLElement)) {
                            switch (options.position) {
                                case 'top':
                                    chart.container.insertBefore(legendElement, chart.container.childNodes[0]);
                                    break;

                                case 'bottom':
                                    chart.container.insertBefore(legendElement, null);
                                    break;
                            }
                        } else {
                            // Appends the legend element as the last child of a given HTMLElement
                            options.position.insertBefore(legendElement, null);
                        }
                    }

                    function addClickHandler(legendElement, legends, seriesMetadata, useLabels) {
                        legendElement.addEventListener('click', function (e) {
                            var li = e.target;
                            if (li.parentNode !== legendElement || !li.hasAttribute('data-legend'))
                                return;
                            e.preventDefault();

                            var legendIndex = parseInt(li.getAttribute('data-legend'));
                            var legend = legends[legendIndex];

                            if (!legend.active) {
                                legend.active = true;
                                li.classList.remove('inactive');
                            } else {
                                legend.active = false;
                                li.classList.add('inactive');

                                var activeCount = legends.filter(function (legend) {
                                    return legend.active;
                                }).length;
                                if (!options.removeAll && activeCount == 0) {
                                    // If we can't disable all series at the same time, let's
                                    // reenable all of them:
                                    for (var i = 0; i < legends.length; i++) {
                                        legends[i].active = true;
                                        legendElement.childNodes[i].classList.remove('inactive');
                                    }
                                }
                            }

                            var newSeries = [];
                            var newLabels = [];

                            for (var i = 0; i < seriesMetadata.length; i++) {
                                if (seriesMetadata[i].legend != -1 && legends[seriesMetadata[i].legend].active) {
                                    newSeries.push(seriesMetadata[i].data);
                                    newLabels.push(seriesMetadata[i].label);
                                }
                            }

                            // Chartist only draws changed data when it gets a new data object
                            chart.update(Object.assign({}, chart.data, {
                                series: newSeries,
                                labels: useLabels ? newLabels : chart.data.labels
                            }));

                            if (options.onClick) {
                                options.onClick(chart, e);
                            }
                        });
                    }

                    removeLegendElement();

                    var legendElement = createLegendElement();
                    var useLabels = chart instanceof Chartist.Pie && chart.data.labels && chart.data.labels.length;
                    var legendNames = getLegendNames(useLabels);
                    var seriesMetadata = initSeriesMetadata(useLabels);
                    var legends = [];

                    // Check if given class names are viable to append to legends
                    var classNamesViable = Array.isArray(options.classNames) && options.classNames.length === legendNames.length;

                    // Loop through all legends to set each name in a list item.
                    legendNames.forEach(function (legend, i) {
                        var legendText = legend.name || legend;
                        var legendSeries = legend.series || [i];

                        var li = createNameElement(i, legendText, classNamesViable);
                        legendElement.appendChild(li);

                        legendSeries.forEach(function (seriesIndex) {
                            seriesMetadata[seriesIndex].legend = i;
                        });

                        legends.push({
                            text: legendText,
                            series: legendSeries,
                            active: true
                        });
                    });

                    chart.on('created', function (data) {
                        appendLegendToDOM(legendElement);
                    });

                    if (options.clickable) {
                        setSeriesClassNames();
                        addClickHandler(legendElement, legends, seriesMetadata, useLabels);
                    }
                };
            };

            return Chartist.plugins.legend;

        }
    )
);

//https://github.com/tmmdata/chartist-plugin-tooltip

(function (root, factory) {
        if (typeof define === 'function' && define.amd) {
            // AMD. Register as an anonymous module.
            define(["chartist"], function (Chartist) {
                return (root.returnExportsGlobal = factory(Chartist));
            });
        } else if (typeof exports === 'object') {
            // Node. Does not work with strict CommonJS, but
            // only CommonJS-like enviroments that support module.exports,
            // like Node.
            module.exports = factory(require("chartist"));
        } else {
            root['Chartist.plugins.tooltip'] = factory(Chartist);
        }
    }
    (this, function (Chartist) {

            /**
             * Chartist.js plugin to display a data label on top of the points in a line chart.
             *
             */
            /* global Chartist */
            (function (window, document, Chartist) {
                'use strict';

                var defaultOptions = {
                    currency: undefined,
                    currencyFormatCallback: undefined,
                    tooltipOffset: {
                        x: 0,
                        y: -20
                    },
                    anchorToPoint: false,
                    appendToBody: false,
                    class: undefined,
                    pointClass: 'ct-point',
                    showSeriesName: false
                };
                
                
                Chartist.plugins = Chartist.plugins || {};
                Chartist.plugins.tooltip = function (options) {
                    options = Chartist.extend({}, defaultOptions, options);

                    return function tooltip(chart) {
                        
                        //set options where included in toolTipOptions
                        options.currency = chart.options.toolTipOptions?.currency;
                        options.showSeriesName = chart.options.toolTipOptions?.showSeriesName;

                        var tooltipSelector = options.pointClass;
                        if (chart instanceof Chartist.Bar) {
                            tooltipSelector = 'ct-bar';
                        } else if (chart instanceof Chartist.Pie) {
                            // Added support for donut graph
                            if (chart.options.donut) {
                                tooltipSelector = 'ct-slice-donut';
                            } else {
                                tooltipSelector = 'ct-slice-pie';
                            }
                        }

                        var $chart = chart.container;
                        var $toolTip = $chart.querySelector('.chartist-tooltip');
                        if (!$toolTip) {
                            $toolTip = document.createElement('div');
                            $toolTip.className = (!options.class) ? 'chartist-tooltip' : 'chartist-tooltip ' + options.class;
                            if (!options.appendToBody) {
                                $chart.appendChild($toolTip);
                            } else {
                                document.body.appendChild($toolTip);
                            }
                        }
                        var height = $toolTip.offsetHeight;
                        var width = $toolTip.offsetWidth;

                        hide($toolTip);

                        function on(event, selector, callback) {
                            $chart.addEventListener(event, function (e) {
                                if (!selector || hasClass(e.target, selector))
                                    callback(e);
                            });
                        }

                        on('mouseover', tooltipSelector, function (event) {
                            var $point = event.target;
                            var tooltipText = '';

                            var isPieChart = (chart instanceof Chartist.Pie) ? $point : $point.parentNode;
                            var seriesName = (isPieChart) ? $point.parentNode.getAttribute('ct:meta') || $point.parentNode.getAttribute('ct:series-name') : '';
                            // Points without meta, such as plain numeric pie slices, fall back to the series name or the pie label
                            var pointMeta = Chartist.deserialize($point.getAttribute('ct:meta'));
                            var metaLabel = pointMeta !== null && typeof pointMeta === 'object' ? pointMeta.label : undefined;
                            var meta = metaLabel || seriesName || '';
                            
                            // if user wants series Name of parent node shown instead of x-axis label
                            meta = options.showSeriesName ? event.target.parentNode.getAttribute('ct:series-name') ?? meta : meta;
                            

                            var hasMeta = !!meta;
                            var value = $point.getAttribute('ct:value') || '';
                            
                            // for stacked bar, take the second value given from ct:value
                            const values = value.split(",");
                            value = values.length > 1 ? values[1].trim() : values[0].trim();
                            
                            if (options.transformTooltipTextFnc && typeof options.transformTooltipTextFnc === 'function') {
                                value = options.transformTooltipTextFnc(value);
                            }

                            if (options.tooltipFnc && typeof options.tooltipFnc === 'function') {
                                tooltipText = options.tooltipFnc(meta, value);
                            } else {
                                if (options.metaIsHTML) {
                                    var txt = document.createElement('textarea');
                                    txt.innerHTML = meta;
                                    meta = txt.value;
                                }

                                meta = '<span class="chartist-tooltip-meta">' + meta + '</span>';

                                if (hasMeta) {
                                    tooltipText += meta + '<br>';
                                } else {
                                    // For Pie Charts also take the labels into account
                                    // Could add support for more charts here as well!
                                    if (chart instanceof Chartist.Pie) {
                                        var label = next($point, 'ct-label');
                                        if (label) {
                                            tooltipText += text(label) + '<br>';
                                        }
                                    }
                                }

                                if (value) {
                                    if (options.currency) {
                                        if (options.currencyFormatCallback != undefined) {
                                            value = options.currencyFormatCallback(value, options);
                                        } else {
                                            value = options.currency + value.replace(/(\d)(?=(\d{3})+(?:\.\d+)?$)/g, '$1,');
                                        }
                                    }
                                    value = '<span class="chartist-tooltip-value">' + value + '</span>';
                                    tooltipText += value;
                                }
                            }

                            if (tooltipText) {
                                $toolTip.innerHTML = tooltipText;
                                setPosition(event);
                                show($toolTip);

                                // Remember height and width to avoid wrong position in IE
                                height = $toolTip.offsetHeight;
                                width = $toolTip.offsetWidth;
                            }
                        });

                        on('mouseout', tooltipSelector, function () {
                            hide($toolTip);
                        });

                        on('mousemove', null, function (event) {
                            if (false === options.anchorToPoint)
                                setPosition(event);
                        });

                        function setPosition(event) {
                            height = height || $toolTip.offsetHeight;
                            width = width || $toolTip.offsetWidth;
                            var offsetX = -width / 2 + options.tooltipOffset.x
                            var offsetY = -height + options.tooltipOffset.y;
                            var anchorX, anchorY;

                            if (!options.appendToBody) {
                                var box = $chart.getBoundingClientRect();
                                var left = event.pageX - box.left - window.pageXOffset;
                                var top = event.pageY - box.top - window.pageYOffset;

                                if (true === options.anchorToPoint && event.target.x2 && event.target.y2) {
                                    anchorX = parseInt(event.target.x2.baseVal.value);
                                    anchorY = parseInt(event.target.y2.baseVal.value);
                                }

                                $toolTip.style.top = (anchorY || top) + offsetY + 'px';
                                $toolTip.style.left = (anchorX || left) + offsetX + 'px';
                            } else {
                                $toolTip.style.top = event.pageY + offsetY + 'px';
                                $toolTip.style.left = event.pageX + offsetX + 'px';
                            }
                        }
                    }
                };

                function show(element) {
                    if (!hasClass(element, 'tooltip-show')) {
                        element.className = element.className + ' tooltip-show';
                    }
                }

                function hide(element) {
                    var regex = new RegExp('tooltip-show' + '\\s*', 'gi');
                    element.className = element.className.replace(regex, '').trim();
                }

                function hasClass(element, className) {
                    return (' ' + element.getAttribute('class') + ' ').indexOf(' ' + className + ' ') > -1;
                }

                function next(element, className) {
                    do {
                        element = element.nextSibling;
                    } while (element && !hasClass(element, className));
                    return element;
                }

                function text(element) {
                    return element.innerText || element.textContent;
                }

            }(window, document, Chartist));

            return Chartist.plugins.tooltip;

        }
    )
);

//https://github.com/gionkunz/chartist-plugin-pointlabels
(function (root, factory) {
        if (typeof define === 'function' && define.amd) {
            // AMD. Register as an anonymous module.
            define(["chartist"], function (Chartist) {
                return (root.returnExportsGlobal = factory(Chartist));
            });
        } else if (typeof exports === 'object') {
            // Node. Does not work with strict CommonJS, but
            // only CommonJS-like enviroments that support module.exports,
            // like Node.
            module.exports = factory(require("chartist"));
        } else {
            root['Chartist.plugins.ctPointLabels'] = factory(Chartist);
        }
    }
    (this, function (Chartist) {

        /**
         * Chartist.js plugin to display a data label on top of the points in a line chart.
         *
         */
        /* global Chartist */
        (function (window, document, Chartist) {
            'use strict';

            var defaultOptions = {
                labelClass: 'ct-label',
                labelOffset: {
                    x: 0,
                    y: -10
                },
                textAnchor: 'middle',
                align: 'center',
                labelInterpolationFnc: Chartist.noop
            };

            var labelPositionCalculation = {
                point: function (data) {
                    return {
                        x: data.x,
                        y: data.y
                    };
                },
                bar: {
                    left: function (data) {
                        return {
                            x: data.x1,
                            y: data.y1
                        };
                    },
                    center: function (data) {
                        return {
                            x: data.x1 + (data.x2 - data.x1) / 2,
                            y: data.y1
                        };
                    },
                    right: function (data) {
                        return {
                            x: data.x2,
                            y: data.y1
                        };
                    }
                }
            };

            Chartist.plugins = Chartist.plugins || {};
            Chartist.plugins.ctPointLabels = function (options) {

                options = Chartist.extend({}, defaultOptions, options);

                function addLabel(position, data) {
                    // if x and y exist concat them otherwise output only the existing value
                    var hasX = data.value.x !== undefined && data.value.x !== null;
                    var hasY = data.value.y !== undefined && data.value.y !== null;
                    var value = hasX && hasY ?
                        (data.value.x + ', ' + data.value.y) :
                        hasY ? data.value.y : data.value.x;

                    data.group.elem('text', {
                        x: position.x + options.labelOffset.x,
                        y: position.y + options.labelOffset.y,
                        style: 'text-anchor: ' + options.textAnchor
                    }, options.labelClass).text(options.labelInterpolationFnc(value));
                }

                return function ctPointLabels(chart) {
                    if (chart instanceof Chartist.Line || chart instanceof Chartist.Bar) {
                        chart.on('draw', function (data) {
                            var positonCalculator = labelPositionCalculation[data.type] && labelPositionCalculation[data.type][options.align] || labelPositionCalculation[data.type];
                            if (positonCalculator) {
                                addLabel(positonCalculator(data), data);
                            }
                        });
                    }
                };
            };

        }(window, document, Chartist));

        return Chartist.plugins.ctPointLabels;

    })
);
