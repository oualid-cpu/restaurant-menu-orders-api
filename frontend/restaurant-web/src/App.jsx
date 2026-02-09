import { useEffect, useState } from "react";
import { getMenu } from "./api";
import OrderPage from "./OrderPage";
import OrdersPage from "./OrdersPage";
import "./App.css";

function MenuPage() {
  const [q, setQ] = useState("");
  const [category, setCategory] = useState("");
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");

  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  async function load() {
    setLoading(true);
    setErr("");
    try {
      const data = await getMenu({ q, category, minPrice, maxPrice });
      setItems(data);
    } catch (e) {
      setErr(e.message || "Something went wrong");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function onSubmit(e) {
    e.preventDefault();
    load();
  }

  function onReset() {
    setQ("");
    setCategory("");
    setMinPrice("");
    setMaxPrice("");
    setTimeout(() => load(), 0);
  }

  return (
    <div className="page">
      <header className="header">
        <h1>Restaurant Menu</h1>
        <p className="sub">
          React website calling your ASP.NET API (<code>/menu</code>)
        </p>
      </header>

      <section className="card">
        <h2>Search & Filter</h2>
        <form className="form" onSubmit={onSubmit}>
          <div className="row">
            <label>
              Search (q)
              <input
                value={q}
                onChange={(e) => setQ(e.target.value)}
                placeholder="e.g. pizza"
              />
            </label>

            <label>
              Category
              <input
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                placeholder="e.g. Pasta"
              />
            </label>
          </div>

          <div className="row">
            <label>
              Min Price
              <input
                value={minPrice}
                onChange={(e) => setMinPrice(e.target.value)}
                placeholder="e.g. 6"
                inputMode="decimal"
              />
            </label>

            <label>
              Max Price
              <input
                value={maxPrice}
                onChange={(e) => setMaxPrice(e.target.value)}
                placeholder="e.g. 10"
                inputMode="decimal"
              />
            </label>
          </div>

          <div className="actions">
            <button type="submit">Apply</button>
            <button type="button" className="secondary" onClick={onReset}>
              Reset
            </button>
          </div>
        </form>
      </section>

      <section className="card">
        <h2>Results</h2>

        {loading && <p>Loading...</p>}
        {err && <p className="error">{err}</p>}

        {!loading && !err && items.length === 0 && (
          <p>No items found. Try removing filters.</p>
        )}

        {!loading && !err && items.length > 0 && (
          <div className="grid">
            {items.map((item) => (
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
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}

export default function App() {
  const [page, setPage] = useState("menu");

  return (
    <>
      <nav className="nav">
        <button
          className={page === "menu" ? "navActive" : ""}
          onClick={() => setPage("menu")}
        >
          Menu
        </button>

        <button
          className={page === "order" ? "navActive" : ""}
          onClick={() => setPage("order")}
        >
          Create Order
        </button>

        <button
          className={page === "orders" ? "navActive" : ""}
          onClick={() => setPage("orders")}
        >
          Orders
        </button>
      </nav>

      {page === "menu" && <MenuPage />}
      {page === "order" && <OrderPage />}
      {page === "orders" && <OrdersPage />}
    </>
  );
}
