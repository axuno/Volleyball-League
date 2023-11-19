/**
 * Polyfill for Element.closest
 */
if (typeof (Element.prototype.closest == 'undefined')) {
    Element.prototype.closest = function (property, value) {
        var x = this;
        while (x = x.parentElement) {
            if (x[property] == value) {
                return this;
            }
        }
        return null;
    }
}
