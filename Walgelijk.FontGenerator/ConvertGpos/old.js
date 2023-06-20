const opentype = require('opentype.js');

opentype.load(process.argv[2], function (err, font) {
    if (err) {
        console.log(err.toString());
        return;
    }

    const cmap = font.tables.cmap.glyphIndexMap;
    let kernings = [];

    const min = 0;
    const max = font.numGlyphs;
    for (let a = min; a < max; a++) {
        const aG = getGlyph(a);
        if (aG)
            for (let b = a + 1; b < max; b++) {
                const bG = getGlyph(b);
                if (bG) {
                    const kerning = font.getKerningValue(aG, bG);
                    if (kerning) {
                        // MsdfKerning struct
                        kernings.push({
                            Unicode1: cmap[aG.index],
                            Unicode2: cmap[bG.index],
                            Advance: kerning
                        });
                    }
                }
            }
    }

    console.log(JSON.stringify(kernings));

    function getGlyph(index) {
        try {
            return font.glyphs.get(index);
        } catch (error) {
            return false;
        }
    }
});
