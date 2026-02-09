import { useEffect, useState } from "react";
import { getOrderById, getOrders } from "./api";

export default function OrdersPage() {
  const [orders, setOrders] = useState([]);
  const [selectedId, setSelectedId] = useState(null);
  const [details, setDetails] = useState(null);

  const [loadingList, setLoadingList] = useState(true);
  const [loadingDetails, setLoadingDetails] = useState(false);
  const [err, setErr] = useState("");

  async function loadOrders() {
    setLoadingList(true);
    setErr("");
    try {
      const data = await getOrders();
      setOrders(data);
      // auto-select first order if none selected
      if (data.length > 0 && selectedId == null) {
        setSelectedId(data[0].id);
      }
    } catch (e) {
      setErr(e.message || "Failed to load orders");
    } finally {
      setLoadingList(false);
    }
  }

  async function loadDetails(id) {
    if (!id) return;
    setLoadingDetails(true);
    setErr("");
    try {
      const data = await getOrderById(id);
      setDetails(data);
    } catch (e) {
      setErr(e.message || "Failed to load order details");
      setDetails(null);
    } finally {
      setLoadingDetails(false);
    }
  }

  useEffect(() => {
    loadOrders();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    if (selectedId != null) loadDetails(selectedId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedId]);

  return (
    <div className="page">
      <header className="header">
        <h1>Orders</h1>
        <p className="sub">
          Orders list (<code>GET /orders</code>) + details (
          <code>GET /orders/{`{id}`}</code>)
        </p>
      </header>

      <section className="card">
        <div className="actions" style={{ justifyContent: "space-between" }}>
          <button type="button" onClick={loadOrders}>
            Refresh Orders
          </button>
          <div style={{ opacity: 0.8 }}>
            Selected: <strong>{selectedId ?? "-"}</strong>
          </div>
        </div>

        {err && <p className="error">{err}</p>}

        <div className="ordersLayout">
          <div className="ordersList">
            <h2>Order List</h2>

            {loadingList && <p>Loading...</p>}

            {!loadingList && orders.length === 0 && (
              <p>No orders yet. Create one in “Create Order”.</p>
            )}

            {!loadingList && orders.length > 0 && (
              <ul className="list">
                {orders.map((o) => (
                  <li key={o.id}>
                    <button
                      type="button"
                      className={
                        selectedId === o.id ? "listBtn active" : "listBtn"
                      }
                      onClick={() => setSelectedId(o.id)}
                    >
                      <div className="listRow">
                        <div>
                          <strong>Order #{o.id}</strong>
                          <div className="small">
                            {new Date(o.createdAtUtc).toLocaleString()}
                          </div>
                        </div>
                        <div style={{ textAlign: "right" }}>
                          <div className="small">Items: {o.itemCount}</div>
                          <div>
                            <strong>€{Number(o.totalAmount).toFixed(2)}</strong>
                          </div>
                        </div>
                      </div>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="ordersDetails">
            <h2>Order Details</h2>

            {loadingDetails && <p>Loading details...</p>}

            {!loadingDetails && !details && (
              <p>Select an order to see details.</p>
            )}

            {!loadingDetails && details && (
              <div className="detailsBox">
                <div className="detailsHeader">
                  <div>
                    <strong>Order #{details.id}</strong>
                    <div className="small">
                      {new Date(details.createdAtUtc).toLocaleString()}
                    </div>
                  </div>
                  <div style={{ textAlign: "right" }}>
                    <div className="small">Total</div>
                    <div className="total">
                      €{Number(details.totalAmount).toFixed(2)}
                    </div>
                  </div>
                </div>

                <div className="table">
                  <div className="thead">
                    <div>Item</div>
                    <div className="right">Qty</div>
                    <div className="right">Unit</div>
                    <div className="right">Line</div>
                  </div>

                  {details.items.map((it) => (
                    <div className="trow" key={it.menuItemId}>
                      <div>
                        <div>
                          <strong>{it.menuItemName}</strong>
                        </div>
                        <div className="small">Id: {it.menuItemId}</div>
                      </div>
                      <div className="right">{it.quantity}</div>
                      <div className="right">
                        €{Number(it.unitPrice).toFixed(2)}
                      </div>
                      <div className="right">
                        €{Number(it.lineTotal).toFixed(2)}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>
      </section>
    </div>
  );
}
