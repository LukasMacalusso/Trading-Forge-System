import os
import glob

def replace_in_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()
    
    new_content = content.replace('using TraderForge.Application.Common;', 'using TraderForge.Domain.Common;')
    
    if new_content != content:
        with open(filepath, 'w') as f:
            f.write(new_content)

if __name__ == '__main__':
    for root, dirs, files in os.walk('.'):
        for file in files:
            if file.endswith('.cs'):
                replace_in_file(os.path.join(root, file))
