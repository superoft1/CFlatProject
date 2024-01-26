#%%
import pandas as pd
import glob


#%%
folder = 'Instruments/KettleTypeHeatExchanger'

for file in glob.glob(folder + '/*.csv'):
    df = pd.read_csv(file, header=None, na_filter=False)
    cols = list(df)
    cols[7], cols[6] = cols[6], cols[7]
    df = df.ix[:, cols]
    print(df.head)
    df.to_csv(file, header=False, index=False)

# with open()

# pd.read_csv()