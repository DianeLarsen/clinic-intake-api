# Git Cheat Sheet

## Clone

```bash
git clone <url>
```

---

## Status

```bash
git status
```

---

## Stage Changes

```bash
git add .
```

Single file

```bash
git add Program.cs
```

---

## Commit

```bash
git commit -m "Add pagination metadata"
```

---

## View History

```bash
git log

git log --oneline
```

---

## Push

```bash
git push
```

---

## Pull

```bash
git pull
```

---

## Branches

Create

```bash
git checkout -b feature/pagination
```

Switch

```bash
git checkout main
```

List

```bash
git branch
```

Merge

```bash
git merge feature/pagination
```

---

## Remote

View

```bash
git remote -v
```

Change

```bash
git remote set-url origin <url>
```

---

## Undo

Discard local changes

```bash
git restore Program.cs
```

Unstage

```bash
git restore --staged Program.cs
```

Amend last commit

```bash
git commit --amend
```

---

## Ignore Files

`.gitignore`

```text
bin/

obj/

.vs/

*.db
```

---

## Daily Workflow

```bash
git pull

git status

git add .

git commit -m "Describe the work"

git push
```

---

## Mental Model

```text
Working Directory

↓

git add

↓

Staging Area

↓

git commit

↓

Local Repository

↓

git push

↓

GitHub
```

---

## Things to Remember

- Commit small, logical changes.
- Write meaningful commit messages.
- Pull before pushing.
- Check `git status` often.
- Git tracks changes, not files.