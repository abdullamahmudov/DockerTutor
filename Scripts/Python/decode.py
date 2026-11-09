import codecs

with open('D:\\Work\\SOCMEDICA\\Projects\\EFDTSL\\efdtsl\\EFDTSL.WebAPI\\Temp\\uploadResult_15-10-2026-18-34-34.json', 'r', encoding='utf-8') as f_in, \
     open('D:\\Work\\SOCMEDICA\\Projects\\EFDTSL\\efdtsl\\EFDTSL.WebAPI\\Temp\\uploadResult_15-10-2026-18-34-34_fix.json', 'w', encoding='utf-8') as f_out:
    for line in f_in:
        f_out.write(line.encode().decode('unicode-escape'))
