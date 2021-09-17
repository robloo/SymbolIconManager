@ECHO OFF
SET scriptPath=%cd%
SET scriptName=list_font_glyphs.py

"C:\Program Files (x86)\FontForgeBuilds\run_fontforge.exe" -script "%scriptPath%\%scriptName%"
