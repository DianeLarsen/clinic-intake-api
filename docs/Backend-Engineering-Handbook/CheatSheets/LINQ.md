# LINQ Cheatsheet

## Filtering

### Where()

Returns only items that match a condition.

```csharp
requests.Where(r => r.Status == RequestStatus.Completed);
```

Example:

```http
GET /requests?status=Completed
```

---

### Contains()

Checks whether a value exists inside a string or collection.

```csharp
requests.Where(r =>
    r.PatientName.Contains(
        patient,
        StringComparison.OrdinalIgnoreCase));
```

Example:

```http
GET /requests?patient=char
```

Matches:

```
Charlie
Charlotte
Richard
```

---

### FirstOrDefault()

Returns the first matching item or `null` if none exists.

```csharp
requests.FirstOrDefault(r => r.Id == id);
```

---

## Counting

### Count()

Returns the number of items.

```csharp
requests.Count();
```

Count matching items:

```csharp
requests.Count(r =>
    r.Status == RequestStatus.Completed);
```

---

## Sorting

### OrderBy()

Ascending order.

```csharp
requests.OrderBy(r => r.PatientName);
```

Result:

```
Alice
Bob
Charlie
Diane
```

---

### OrderByDescending()

Descending order.

```csharp
requests.OrderByDescending(r => r.PatientName);
```

Result:

```
Diane
Charlie
Bob
Alice
```

---

## Common Query Pattern

```csharp
IEnumerable<IntakeRequest> requests =
    _repository.GetAll();

if (status is not null)
{
    requests = requests.Where(r =>
        r.Status == status.Value);
}

if (!string.IsNullOrWhiteSpace(patient))
{
    requests = requests.Where(r =>
        r.PatientName.Contains(
            patient,
            StringComparison.OrdinalIgnoreCase));
}

if (sort == "name")
{
    requests = requests.OrderBy(r =>
        r.PatientName);
}

return requests;
```

---

## Methods Learned So Far

| Method                | Purpose              |
| --------------------- | -------------------- |
| `Where()`             | Filter items         |
| `Contains()`          | Partial text search  |
| `FirstOrDefault()`    | First item or `null` |
| `Count()`             | Count items          |
| `OrderBy()`           | Sort ascending       |
| `OrderByDescending()` | Sort descending      |

---

## Coming Next

* `Skip()`
* `Take()`
* `Select()`
* `Any()`
* `All()`
* `ThenBy()`
* `ThenByDescending()`
* `SelectMany()`
* `Distinct()`
* `GroupBy()`
* `ToDictionary()`
* `ToList()`
* `Sum()`
* `Average()`
* `Min()`
* `Max()`
* `Join()`
* `Any()`
* `Single()`
* `SingleOrDefault()`

---

## Mental Reminder

Most LINQ methods **return a new sequence**.

They do **not** modify the original collection.

```csharp
requests = requests.Where(...);
```

not

```csharp
requests.Where(...);
```

---

## Translation

| Plain English   | LINQ                  |
| --------------- | --------------------- |
| Keep only these | `Where()`             |
| Find one        | `FirstOrDefault()`    |
| Count them      | `Count()`             |
| Sort A → Z      | `OrderBy()`           |
| Sort Z → A      | `OrderByDescending()` |
| Search text     | `Contains()`          |
