import os
import zipfile
import shutil

zip_filename = 'Midterm_Darsavelidze_Rezo.zip'

include_roots = [
    'GeorgiaTransitIntelligence',
    'GeorgiaTransitIntelligence.Tests',
    'docs',
    'screenshots',
    'data',
    'web-preview',
    'scripts'
]

root_files = [
    'GeorgiaTransitIntelligence.sln',
    'GeorgiaTransitIntelligence.slnx',
    'database.sql',
    'README.md'
]

exclude_dirs = {'bin', 'obj', '.vs', '.git', '.idea', '__pycache__', 'node_modules'}
exclude_extensions = {'.user', '.suo', '.tmp'}

if os.path.exists(zip_filename):
    try:
        os.remove(zip_filename)
    except Exception as e:
        print('Notice on existing zip remove:', e)

print(f'Creating clean university submission zip: {zip_filename}')

with zipfile.ZipFile(zip_filename, 'w', zipfile.ZIP_DEFLATED) as zipf:
    for rf in root_files:
        if os.path.exists(rf):
            zipf.write(rf, rf)
            print(f'  + Root file: {rf}')

    for folder in include_roots:
        if not os.path.exists(folder):
            continue
        for root, dirs, files in os.walk(folder):
            dirs[:] = [d for d in dirs if d not in exclude_dirs]
            for file in files:
                if any(file.endswith(ext) for ext in exclude_extensions):
                    continue
                if file.endswith('.zip'):
                    continue
                full_path = os.path.join(root, file)
                # Exclude raw dumps > 2MB to keep archive lightweight and fast
                if os.path.getsize(full_path) > 2_000_000:
                    continue
                rel_path = os.path.relpath(full_path, '.')
                zipf.write(full_path, rel_path)

size_bytes = os.path.getsize(zip_filename)
print(f'Created {zip_filename} successfully! Size: {size_bytes:,} bytes ({size_bytes / 1024:.1f} KB)')

# Copy to web-preview for direct browser download button
web_dest = os.path.join('web-preview', zip_filename)
shutil.copy2(zip_filename, web_dest)
print(f'Copied {zip_filename} to web-preview/ directory.')
