// All Site scripts go into the same namespace
if (Site === undefined) {
    var Site = {};
}

Site.TempusDominusFactory = class {
    'use strict';
    /**
    * Represents a TempusDominusFactory.
    * @constructor
    * @param {string} locale - The locale name including region, e.g. "en-US".
    * @param {string} fallbackLocale - The fallback locale to use, e.g. "en".
    * @param {int} hourCycle - The hour cycle, which can be 12 or 24. Used to make TempusDominus behave like .NET
    * @param {boolean} useBiIcons - True, when Bootstrap Icons shall be used
    */
    constructor(locale, fallbackLocale, hourCycle, useBiIcons) {
        this.tdLocale = tempusDominus.locales[locale] || tempusDominus.locales[fallbackLocale];
        // .NET always uses ante meridiem designator for hours 0:00:00 (midnight) to 11:59:59.999,
        // and post meridiem designator for later hours (after noon).
        this.tdHourCycle = hourCycle == 12 ? 'h11' : 'h23';
        this.useBiIcons = useBiIcons;

        this._setTdDefaults();
    }

    _setTdDefaults() {
        if (this.tdLocale) {
            // Set the locale, if it's not the default (en):
            tempusDominus.DefaultOptions.localization = this.tdLocale.localization;
            tempusDominus.DefaultOptions.localization.hourCycle = this.tdHourCycle;
        }
        
        // Change year to 4 digits instead of 2:
        tempusDominus.DefaultOptions.localization.dayViewHeaderFormat = { month: 'long', year: 'numeric' };

        if (this.useBiIcons) {
            tempusDominus.DefaultOptions.display.icons = {
                time: 'bi bi-clock',
                date: 'bi bi-calendar',
                up: 'bi bi-arrow-up',
                down: 'bi bi-arrow-down',
                previous: 'bi bi-chevron-left',
                next: 'bi bi-chevron-right',
                today: 'bi bi-calendar-check',
                clear: 'bi bi-trash',
                close: 'bi bi-x bi-lg'
            };
        } else {
            tempusDominus.DefaultOptions.display.icons = {
                time: 'fas fa-clock',
                date: 'fas fa-calendar',
                up: 'fas fa-arrow-up',
                down: 'fas fa-arrow-down',
                previous: 'fas fa-chevron-left',
                next: 'fas fa-chevron-right',
                today: 'fas fa-calendar-check',
                clear: 'fas fa-trash',
                close: 'fas fa-times',
            };
        }
        tempusDominus.DefaultOptions.display.buttons = {
            today: false,
            clear: true,
            close: true
        };
    }

    /**
     * Creates a new calendar picker and adds it to the {element} as '_tempusDominus'.
     * If the element's value is not valid, the value is removed.
     * @param {HTMLElement} element
     * @param {string} format - Optional: The input format either as TempusDominus constant (e.g.: "L") or custom format (e.g.: "yyyy-MM-dd")
     */
    CreateCalendarPicker(element, format) 
    {
        if (format === undefined) {
            format = 'L';
        }

        try {
            try {
                // Destroy any existing widget
                if (element._tempusDominus) element._tempusDominus.dispose();
            }
            catch {}
            
            element._tempusDominus = this._tryCreateCalendarPicker(element, format);
        } catch {
            element.querySelector('input').value = '';
            element._tempusDominus = this._tryCreateCalendarPicker(element, format);
        }
    }

    /**
     * Creates a new time picker and adds it to the {element} as '_tempusDominus'.
     * If the element's value is not valid, the value is removed.
     * @param {HTMLElement} element
     * @param {string} format - Optional: The input format either as TempusDominus constant (e.g.: "LT") or custom format (e.g.: "h:mm")
     */
    CreateTimePicker(element, format) 
    {
        if (format === undefined) {
            format = 'LT';
        }

        try {
            try {
                // Destroy any existing widget
                if (element._tempusDominus) element._tempusDominus.dispose();
            }
            catch {}
            
            element._tempusDominus = this._tryCreateTimePicker(element, format);
        } catch {
            element.querySelector('input').value = '';
            element._tempusDominus = this._tryCreateTimePicker(element, format);
        }
    }

    /**
     * Lets the 'Escape' key close the specified picker {elements}
     * @param {Array.<HTMLElement>} elements
     */
    SetEscapeKeyClosesPicker(elements) 
    {
        document.addEventListener('keydown', function (event) {
            if (event.key !== 'Escape') return;
            [].forEach.call(elements, function (el) {
                const td = el._tempusDominus;
                if (td) {
                    td.hide();
                }
            });
        });
    }

    /**
     * Tries to create a new calendar picker.
     * Creation will throw for invalid values.
     * @param {HTMLElement} element
     * @param {string} format - The input format either as TempusDominus constant (e.g.: "L") or custom format (e.g.: "yyyy-MM-dd")
     * @returns {tempusDominus.TempusDominus}
     */
    _tryCreateCalendarPicker(element, format)
    {
        const calPicker = new tempusDominus.TempusDominus(element, {
            container: element,
            allowInputToggle: false,
            localization: {
                format: format
            },
            display: {
                viewMode: 'calendar',
                components: {
                    clock: false
                },
            },
            restrictions: {
            }
        });

        this._fixUncaughtExceptions(calPicker);
        return calPicker;
    }

    /**
     * Tries to create a new time picker.
     * Creation will throw for invalid values.
     * @param {HTMLElement} element
     * @param {string} format - Optional: The input format either as TempusDominus constant (e.g.: "LT") or custom format (e.g.: "h:mm")
     * @returns {tempusDominus.TempusDominus}
     */
    _tryCreateTimePicker(element, format)
    {
        const timePicker = new tempusDominus.TempusDominus(element, {
            container: element,
            allowInputToggle: false,
            stepping: 15,
            localization: {
                format: format
            },
            display: {
                viewMode: 'clock',
                components: {
                    decades: false,
                    year: false,
                    month: false,
                    date: false,
                    hours: true,
                    minutes: true,
                    seconds: false
                },
                buttons: {
                    today: false,
                    clear: true,
                    close: true
                }
            }
        });

        this._fixUncaughtExceptions(timePicker);
        return timePicker;
    }


    /**
    * TempusDominus v6.7.10 throws when invalid dates/times
    * are entered. This workaround catches the exception by
    * overriding the default 'parseInput' method.
    * See: https://github.com/Eonasdan/tempus-dominus/discussions/2656#discussioncomment-5713755
    * @param {tempusDominus.TempusDominus} widget - The widget instance to be fixed.
    */
    _fixUncaughtExceptions (widget) 
    {
        widget.dates.origParseInput = widget.dates.parseInput;
        widget.dates.parseInput = (input) => {
            try {
                return widget.dates.origParseInput(input);
            }
            catch (err) {
                widget.dates.clear();
            }
        };
    }
}
