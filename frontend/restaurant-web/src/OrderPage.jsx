import { useEffect, useMemo, useState } from "react";
import { createOrder, getMenu } from "./api";

export default function OrderPage() {
  const [menu, setMenu] = useState([]);
  const [qty, setQty] = useState({}); // { [menuItemId]: number }
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [success, setSuccess] = useState(null);

  async function loadMenu() {
    setLoading(true);
    setErr("");
    try {
      const items = await getMenu();
      setMenu(items);
    } catch (e) {
      setErr(e.message || "Failed to load menu");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadMenu();
  }, []);

  const selectedItems = useMemo(() => {
    return Object.entries(qty)
      .map(([id, q]) => ({ menuItemId: Number(id), quantity: Number(q) }))
      .filter((x) => x.quantity > 0);
  }, [qty]);

  const estimatedTotal = useMemo(() => {
    let total = 0;
    for (const line of selectedItems) {
      const item = menu.find((m) => m.id === line.menuItemId);
      if (!item) continue;
      total += Number(item.price) * line.quantity;
    }
    return total;
  }, [selectedItems, menu]);

  function setQuantity(menuItemId, value) {
    const n = Number(value);
    setQty((prev) => ({
      ...prev,
      [menuItemId]: Number.isFinite(n) ? n : 0,
    }));
  }

  async function submitOrder() {
    setErr("");
    setSuccess(null);

    if (selectedItems.length === 0) {
      setErr("Please choose at least one item (quantity > 0).");
      return;
    }

    try {
      const result = await createOrder(selectedItems);
      setSuccess(result);
      setQty({});
    } catch (e) {
      setErr(e.message || "Failed to create order");
    }
  }

  return (
    <div className="page">
      <header className="header">
        <h1>Create Order</h1>
        <p className="sub">
          Choose quantities and send <code>POST /orders</code>
        </p>
      </header>

      <section className="card">
        <div className="actions" style={{ justifyContent: "space-between" }}>
          <button onClick={loadMenu} type="button">
            Reload Menu
          </button>
          <button onClick={submitOrder} type="button">
            Create Order
          </button>
        </div>

        {loading && <p>Loading menu...</p>}
        {err && <p className="error">{err}</p>}

        {success && (
          <div className="successBox">
            <strong>Order created ✅</strong>
            <div>Id: {success.id}</div>
            <div>Total: €{Number(success.totalAmount).toFixed(2)}</div>
            <div style={{ opacity: 0.8 }}>
              CreatedAtUtc: {success.createdAtUtc}
            </div>
          </div>
        )}

        {!loading && menu.length > 0 && (
          <div className="grid">
            {menu.map((item) => (
              <div key={item.id} className="menuItem">
                <div className="top">
                  <strong>{item.name}</strong>
                  <span className="price">
                    €{Number(item.price).toFixed(2)}
                  </span>
                </div>

                <div className="meta">
                  <span className="badge">{item.category}</span>
                  <span className={item.isAvailable ? "ok" : "no"}>
                    {item.isAvailable ? "Available" : "Not available"}
                  </span>
                </div>

                {item.description && <p className="desc">{item.description}</p>}

                <label style={{ marginTop: 10 }}>
                  Quantity
                  <input
                    type="number"
                    min="0"
                    step="1"
                    value={qty[item.id] ?? 0}
                    onChange={(e) => setQuantity(item.id, e.target.value)}
                  />
                </label>
              </div>
            ))}
          </div>
        )}

        {!loading && menu.length === 0 && !err && <p>No menu items found.</p>}
      </section>

      <section className="card">
        <h2>Order Preview</h2>

        {selectedItems.length === 0 && <p>No items selected yet.</p>}

        {selectedItems.length > 0 && (
          <>
            <ul>
              {selectedItems.map((line) => {
                const item = menu.find((m) => m.id === line.menuItemId);
                return (
                  <li key={line.menuItemId}>
                    {item ? item.name : `Item ${line.menuItemId}`} — qty{" "}
                    {line.quantity}
                  </li>
                );
              })}
            </ul>

            <p>
              Estimated total (frontend):{" "}
              <strong>€{estimatedTotal.toFixed(2)}</strong>
            </p>
            <p style={{ opacity: 0.8 }}>
              Backend will compute the real total server-side.
            </p>
          </>
        )}
      </section>
    </div>
  );
}
