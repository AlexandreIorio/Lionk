import subprocess
import sys
import os


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
    with open(filename, 'r') as file:
        return [line.strip() for line in file.readlines()]


app_path = os.getenv('APP_PATH')
app_name = os.getenv('APP_NAME')
bot_name = os.getenv('BOT_NAME')
bot_mail = os.getenv('BOT_MAIL')


with open("newversion.txt", 'r') as file:
    newversion = file.read().strip()


run_command(['git', 'config', '--global', 'user.name', bot_name])
run_command(['git', 'config', '--global', 'user.email', bot_mail])


tag = f"{app_name}_{newversion}"

print(f"Creating tag {tag}")


# Create the tag

run_command(['git', 'tag', '-a', tag, '-m', f"Release {tag}"])
run_command(['git', 'push', 'origin', tag])

# Extract description from description.txt
description = ""
with open("description.txt", 'r') as file:
    description = file.read()

# Create the release with the description
run_command(['gh', 'release', 'create', tag, '--title', tag, '--notes', description])

