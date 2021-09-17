import fontforge
import os

# Relative to the 'Source' directory
relative_sources = [
"Data\LineAwesome\la-brands-400.ttf",
"Data\LineAwesome\la-regular-400.ttf",
"Data\LineAwesome\la-solid-900.ttf"]

for src in relative_sources:

    src_path = os.path.join(os.path.dirname(os.getcwd()), src)
    dst_name = os.path.splitext(os.path.basename(src_path))[0] + '.json'
    dst_path = os.path.join(os.path.dirname(src_path), dst_name)

    font = fontforge.open(src_path)
    output = open(dst_path, "w") # Will overwrite
	
    # Using glyph iterator methadology from:
    # https://fontforge.org/docs/scripting/python/fontforge.html#fontforge.font.glyphs
    # https://github.com/fontforge/fontforge/blob/master/tests/test1008.py

    output.write('{\n')

    for glyph in font.glyphs():
        if glyph.glyphname != '.notdef': # Exclude standard undefined glyphs
            output.write('  \"' + hex(glyph.unicode) + '\": \"' + glyph.glyphname + '\",\n')

    output.write('}')

    # Cleanup and close
    output.close()
    font.close()
