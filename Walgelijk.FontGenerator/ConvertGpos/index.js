const fontkit = require('fontkit');
const fs = require('fs');

const charset = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_\`abcdefghijklmnopqrstuvwxyz{|}~ ��������������������\����������������������������������������������������������������������������";
const font = fontkit.openSync(process.argv[2]);
//const charset = font.characterSet.map(n => String.fromCodePoint(n));

let kernings = [];

for (var b = 0; b < charset.length; b++)
    for (var a = 0; a < charset.length; a++) {
        getKerning(charset[a], charset[b]);
    }

fs.writeFileSync(process.argv[3], JSON.stringify(kernings), { encoding: 'utf8', })

function getKerning(left, right) {

    const baseAdvance = font.layout(left).positions[0].xAdvance;
    const advance = font.layout(left + right).positions[0].xAdvance;
    let kerning = advance - baseAdvance;

    if (kerning) {
        kerning /= font.unitsPerEm;
        kernings.push({
            Unicode1: left.charCodeAt(0),
            Unicode2: right.charCodeAt(0),
            Advance: kerning
        });
    }
}

