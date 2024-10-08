import subprocess
import sys
import os
import json


def run_command(command):
    try:
        result = subprocess.run(command, check=True, capture_output=True, text=True)
        print("STDOUT:", result.stdout.strip())
        print("STDERR:", result.stderr.strip())
    except subprocess.CalledProcessError as e:
        print(f"Error running command {command}: {e}")
        print("STDOUT:", e.stdout)
        print("STDERR:", e.stderr)
        sys.exit(1)

def read_file_to_list(filename):
    try:
        with open(filename, 'r') as file:
            return file.read().strip().split()
    except FileNotFoundError:
        print(f"File not found: {filename}")
        sys.exit(1)

bot_name = os.getenv('BOT_NAME')
bot_mail = os.getenv('BOT_MAIL')
changelogs = json.loads(os.getenv("CHANGELOG"))

projects = read_file_to_list('projects.txt')
newversions = read_file_to_list('newversions.txt')

print(f"Projects: {projects}")
print(f"New versions: {newversions}")

if len(projects) == 0:
    print("No projects to process")
    sys.exit(1)

run_command(['git', 'config', '--global', 'user.name', bot_name])
run_command(['git', 'config', '--global', 'user.email', bot_mail])

for i, project in enumerate(projects):
    newversion = newversions[i]
    tag = f"{project}_{newversion}"

    print(f"Creating tag {tag}")

    # Create the tag

    run_command(['git', 'tag', '-a', tag, '-m', f"Release {tag}"])
    run_command(['git', 'push', 'origin', tag])

    # Extract description from description.txt
    changelog = ""
    if project in changelogs:
        changes = f"## {newversion} Changelog"
        for change in changelogs[project]:
            changes += f"\n- {change}"

    # Create the release with the description
    run_command(['gh', 'release', 'create', tag, '--title', tag, '--notes', changes])

