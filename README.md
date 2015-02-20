# cuesheet-splitter-encoder
A program to split audio files based on a cue sheet and encode the output.

### Example
```
cse.exe -e fhgaacenc:3 -i "C:\path-to-cue\cuesheet.cue" -o "C:\output-path" -c "C:\path-to-cover\cover.jpg"
```

### Usage
```
-h, --help                 Show this message and exit.
-e, --encoder=ENCODER TYPE:QUALITY
                           The ENCODER TYPE and QUALITY value to use.

                             Valid ENCODER TYPE values are:

                             fhgaacenc
                             nero
                             qaac
                             qaac64

                             Numeric QUALITY values are specific to each
                             encoder (fhgaacenc uses a value between 1 and 6,
                             qaac uses 0 to 127, etc.)

                             VBR mode is always used.

                             These encoders are not distributed with this
                             program. They must be installed separately and
                             copied to the executable directory or made
                             accessible via the System PATH environment
                             variable. Visit http://wiki.hydrogenaud.io/index.
                             php?title=AAC_encoders to learn more about them.
-i, --input=PATH           The PATH to the cue sheet file.

                             FLAC, WavPack and Monkey's Audio* files can be
                             split. Decoders for these files are not
                             distributed with this program. They must be
                             installed separately and copied to the
                             executable directory or made accessible via the
                             System PATH environment variable.

                             *Ensure the FLAC decoder is installed if
                             splitting Monkey's Audio files. MAC.exe does not
                             provide any splitting functionality so a
                             transcode to FLAC is required.
-o, --output=PATH          The output PATH.
-c, --cover=PATH           The PATH to a front cover image.
```
