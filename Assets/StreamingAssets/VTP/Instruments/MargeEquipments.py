import os

for dir in os.listdir('.'):
    if not os.path.isdir(dir):
        continue
    file, _ = os.path.splitext(os.path.basename(dir))
    with open(file + 's.csv', 'w') as f:
        if file == 'GenericEquipment':
            print(file)
        for csv in os.listdir(dir):
            root, ext = os.path.splitext(csv)
            if ext != '.csv':
                continue
            with open(os.path.join(file, csv), 'r') as fr:
                for line in fr:
                    f.write(line + '\n')
