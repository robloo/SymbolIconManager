import fontforge
import os

# This script uses FontForge to list all glyph Unicode points and names in a font file.
#
# Some icon set families, such as Line Awesome, do not provide good UnicodePoint + Name
# information for each glyph. Line Awesome, for example, provides this information in a hard
# to process CSS file. Names of which are not guaranteed to align with the glyphs or SVG sources.
#
# This script is designed to work around this issue by reading a font directly and extracting
# all glyph information including Unicode point and name.

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

    # Using glyph iterator methadology from:
    # https://fontforge.org/docs/scripting/python/fontforge.html#fontforge.font.glyphs
    # https://github.com/fontforge/fontforge/blob/master/tests/test1008.py

    output = ''
    output += '{\n'

    for glyph in font.glyphs():
        if glyph.glyphname != '.notdef': # Exclude standard undefined glyphs
            output += '  \"' + hex(glyph.unicode) + '\": \"' + glyph.glyphname + '\",\n'

    output += '}'
    
    # A very simply technique to remove the last comma ',' in the data
    # Some JSON parsers fail if there is a comma at the end with no additional entry
    if output.endswith('\",\n}'):
        output = output.replace('\",\n}', '\"\n}')

    output_file = open(dst_path, "w") # Will overwrite
    output_file.write(output)
    output_file.close()

    font.close()
