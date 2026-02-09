const API_BASE = "http://localhost:5085";
// 👆 change if your API port is different

export async function getMenu(params = {}) {
  const url = new URL(`${API_BASE}/menu`);

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === "") return;
    url.searchParams.set(key, value);
  });

  const res = await fetch(url.toString());
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`GET /menu failed (${res.status}): ${text}`);
  }
  return res.json();
}

export async function createOrder(items) {
  const res = await fetch(`${API_BASE}/orders`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ items }),
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`POST /orders failed (${res.status}): ${text}`);
  }
  return res.json();
}

export async function getOrders() {
  const res = await fetch(`${API_BASE}/orders`);
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`GET /orders failed (${res.status}): ${text}`);
  }
  return res.json();
}

export async function getOrderById(id) {
  const res = await fetch(`${API_BASE}/orders/${id}`);
  if (!res.ok) {
    const text = await res.text();
    throw new Error(`GET /orders/${id} failed (${res.status}): ${text}`);
  }
  return res.json();
}
