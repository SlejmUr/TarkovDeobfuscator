import json

remapfile = open('AutoRemapperConfig.json', 'r')

rem_read = remapfile.readlines()

shit = ""

for rem1 in rem_read:
    if not rem1.__contains__("//"):
        shit += rem1


f = open("AutoRemapperConfig_cleaned.json", "w")
f.write(shit)
f.close()

print(shit)
jsony = json.loads(shit)
thigy = []


for i in jsony['DefinedRemapping']:
    if i["RenameClassNameTo"] in thigy:
        print(i["RenameClassNameTo"])
    else:
        thigy.append(i["RenameClassNameTo"])
