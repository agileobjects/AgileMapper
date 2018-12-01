$(document).ready(function () {
    var hlCode = document.querySelectorAll('pre code.cs'),
        i,
        hlLength = hlCode.length,
        mapperRegex = new RegExp('\\bMapper\\b', 'g'),
        typeRegex = new RegExp('(new</span>\\W+|class</span> <span class="hljs-title">|&lt;)([A-Z][^&\\\( ]+)( |{|\\\(|&gt;)', 'g'),
        genericTypeRegex = new RegExp('(IDictionary|Dictionary|IEnumerable|IReadOnlyCollection|Collection|List)&lt;', 'g'),
        observer = new MutationObserver(function (mutations) {
            for (var mutation of mutations) {
                if (mutation.attributeName === 'class') {
                    var innerHTML = mutation.target.innerHTML
                        .replace(mapperRegex, '<span class="hljs-type">Mapper</span>')
                        .replace(typeRegex, '$1<span class="hljs-type">$2</span>$3')
                        .replace(genericTypeRegex, '<span class="hljs-type">$1</span>&lt;');
                    mutation.target.innerHTML = innerHTML;
                }
            }
        }),
        config = { attributes: true };

    for (i = 0; i < hlLength; ++i) {
        observer.observe(hlCode[i], config);
    }
});